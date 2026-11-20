using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Web.Features.Catalog;

public sealed record WatchImportOutcome(int Line, string Status, string? WorkId, string? PosterStatus = null);

public sealed class WatchListImporter(DataPaths paths, TmdbMovies tmdb, TmdbPosterCache? posters = null, bool skipPosters = false)
{
    private const string Kind = "catalog.watch_list";
    private const string Language = "pt-BR";
    private readonly CommandRunner runner = new(paths);
    private readonly TrackingService tracking = new(paths);

    public async Task<IReadOnlyList<WatchImportOutcome>> RunAsync(IReadOnlyList<WatchListItem> items,
        Action<WatchImportOutcome> report, CancellationToken ct = default)
    {
        var outcomes = new List<WatchImportOutcome>();
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            var operation = OperationId(item);
            var receipt = await FindReceiptAsync(operation, ct);
            if (receipt is null)
            {
                Match match;
                try { match = await ResolveAsync(item, ct); }
                catch (DomainException error) when (error.Code.StartsWith("tmdb_", StringComparison.Ordinal))
                {
                    Publish(new(item.Line, error.Code, null));
                    break;
                }
                if (match.Media is null)
                {
                    Publish(new(item.Line, match.Status, null));
                    continue;
                }

                try
                {
                    receipt = await runner.RunAsync(operation, Kind, operation,
                        (db, owner) => ImportAsync(db, owner, item, match, operation, ct), ct);
                }
                catch (DomainException error) when (error.Code == "review_local_duplicate")
                {
                    Publish(new(item.Line, error.Code, null));
                    continue;
                }
                catch (DbUpdateException)
                {
                    Publish(new(item.Line, "watch_list_write_failed", null));
                    continue;
                }
            }

            Publish(new(item.Line, receipt.Status, receipt.WorkId));
        }
        return await ReportPostersAsync(outcomes, ct);

        void Publish(WatchImportOutcome outcome)
        {
            outcomes.Add(outcome);
            report(outcome);
        }
    }

    public async Task<IReadOnlyList<WatchImportOutcome>> ResumePostersAsync(IReadOnlyList<WatchListItem> items, CancellationToken ct = default)
    {
        var outcomes = new List<WatchImportOutcome>();
        foreach (var item in items)
        {
            var receipt = await FindReceiptAsync(OperationId(item), ct);
            outcomes.Add(receipt is null ? new(item.Line, "not_imported", null, "not_imported")
                : new(item.Line, receipt.Status, receipt.WorkId));
        }
        return await ReportPostersAsync(outcomes, ct);
    }

    private async Task<IReadOnlyList<WatchImportOutcome>> ReportPostersAsync(IReadOnlyList<WatchImportOutcome> outcomes, CancellationToken ct)
    {
        var workIds = outcomes.Select(outcome => outcome.WorkId).OfType<string>().Distinct().ToArray();
        if (workIds.Length == 0) return outcomes;

        await using var db = new AppDbContext(paths);
        var works = await db.Works.AsNoTracking().Where(work => workIds.Contains(work.Id)).ToListAsync(ct);
        var statuses = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        string? pauseReason = null;
        await Parallel.ForEachAsync(works, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
            async (work, cancellation) =>
            {
                var status = await RefreshCoverAsync(work, Volatile.Read(ref pauseReason), cancellation);
                if (status is "tmdb_rate_limit" or "tmdb_authentication" or "tmdb_configuration")
                    Interlocked.CompareExchange(ref pauseReason, status, null);
                statuses[work.Id] = status;
            });

        return outcomes.Select(outcome => outcome.WorkId is not { } id ? outcome : outcome with
        {
            PosterStatus = statuses.GetValueOrDefault(id, "work_missing") switch
            {
                "no_link" when outcome.Status == "local_not_found" => "no_match",
                "no_link" when outcome.Status == "local_ambiguous" => "ambiguous_match",
                var status => status
            }
        }).ToArray();
    }

    private async Task<Receipt?> FindReceiptAsync(string operation, CancellationToken ct)
    {
        await using var db = new AppDbContext(paths);
        var result = await db.Receipts.Where(receipt => receipt.Kind == Kind && receipt.OperationId == operation)
            .Select(receipt => receipt.Result).SingleOrDefaultAsync(ct);
        return result is null ? null : JsonSerializer.Deserialize<Receipt>(result);
    }

    private async Task<Receipt> ImportAsync(AppDbContext db, string owner, WatchListItem item, Match match,
        string operation, CancellationToken ct)
    {
        var media = match.Media!;
        var work = match.ExternalId is int id
            ? await TmdbCatalog.BySource(db, media, id).SingleOrDefaultAsync(ct) : null;
        if (work is null)
        {
            var titles = new[] { WatchList.TitleKey(item.Title), WatchList.TitleKey(item.AlternateTitle) };
            var local = (await db.Works.Where(entry => entry.MediaCode == media && entry.ReleaseYear == item.Year).ToListAsync(ct))
                .Where(entry => titles.Contains(WatchList.TitleKey(entry.Title))).Take(2).ToArray();
            if (local.Length > 1 || local is [var one] && match.ExternalId is int externalId && SourceId(one) is int prior && prior != externalId)
                throw new DomainException("review_local_duplicate");
            work = local.SingleOrDefault();
        }

        if (work is null)
        {
            work = new Work
            {
                Id = Guid.NewGuid().ToString(), MediaCode = media, IsAnime = item.IsAnime,
                Title = item.Title, Description = item.Description, ReleaseYear = item.Year
            };
            db.Works.Add(work);
        }
        if (match.ExternalId is int sourceId && SourceId(work) is null)
        {
            if (media == "filme") work.TmdbMovieId = sourceId;
            else work.TmdbSeriesId = sourceId;
        }
        if (item.IsAnime) work.IsAnime = true;

        if (!await db.Trackings.AnyAsync(row => row.IdentityId == owner && row.WorkId == work.Id, ct))
            await tracking.AddCoreAsync(db, owner, new(operation, work.Id, "planned", new(null, null, false)), ct);

        return new(work.Id, media, SourceId(work), match.Status);
    }

    private async Task<Match> ResolveAsync(WatchListItem item, CancellationToken ct)
    {
        var mediaToSearch = item.MediaCode is { } media ? [media] : new[] { "filme", "serie" };
        var queries = new[] { item.Title, item.AlternateTitle }.DistinctBy(WatchList.TitleKey).ToArray();
        var titles = queries.Select(WatchList.TitleKey).ToHashSet(StringComparer.Ordinal);
        var candidates = new HashSet<(string Media, int Id)>();
        var incomplete = false;

        foreach (var type in mediaToSearch)
        foreach (var query in queries)
        {
            const int pageLimit = 2;
            var totalPages = 1;
            for (var page = 1; page <= Math.Min(totalPages, pageLimit); page++)
            {
                if (type == "filme")
                {
                    var result = await WithRetryAsync(() => tmdb.SearchAsync(query, item.Year, page, Language, ct), ct);
                    totalPages = result.TotalPages;
                    foreach (var movie in result.Movies) Add(type, movie.Id, movie.Title, movie.OriginalTitle, movie.Year);
                }
                else
                {
                    var result = await WithRetryAsync(() => tmdb.SearchSeriesAsync(query, item.Year, page, Language, ct), ct);
                    totalPages = result.TotalPages;
                    foreach (var series in result.Series) Add(type, series.Id, series.Title, series.OriginalTitle, series.Year);
                }
                incomplete |= totalPages > pageLimit;
            }
        }

        if (!incomplete && candidates.Count == 1)
        {
            var selected = candidates.Single();
            return new(selected.Media, selected.Id, "linked");
        }
        if (item.MediaCode is null) return new(null, null, "review_anime_type");
        return new(item.MediaCode, null, incomplete || candidates.Count > 1 ? "local_ambiguous" : "local_not_found");

        void Add(string media, int id, string title, string? original, int? year)
        {
            if (year == item.Year && (titles.Contains(WatchList.TitleKey(title)) || original is not null && titles.Contains(WatchList.TitleKey(original))))
                candidates.Add((media, id));
        }
    }

    private async Task<string> RefreshCoverAsync(Work work, string? pauseReason, CancellationToken ct)
    {
        if (TmdbPosterCache.HasLocalCover(paths, work)) return "local_cover";
        if (SourceId(work) is not int id) return "no_link";
        if (posters?.Find(work.MediaCode, id) is not null) return "cached";
        if (posters is null || skipPosters) return "skipped";
        if (pauseReason is not null) return "deferred_" + pauseReason;
        try
        {
            var url = work.MediaCode == "filme"
                ? (await WithRetryAsync(() => tmdb.GetMovieAsync(id, Language, ct), ct)).PosterUrl
                : (await WithRetryAsync(() => tmdb.GetSeriesAsync(id, Language, ct), ct)).PosterUrl;
            if (url is null) return "no_poster_source";
            return await posters.TryStoreAsync(work.MediaCode, id, url, ct) ? "downloaded" : "download_failed";
        }
        catch (DomainException error) when (error.Code.StartsWith("tmdb_", StringComparison.Ordinal)) { return error.Code; }
    }

    private static async Task<T> WithRetryAsync<T>(Func<Task<T>> request, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            await Task.Delay(120, ct);
            try { return await request(); }
            catch (DomainException error) when (attempt < 2 && error.Code is "tmdb_rate_limit" or "tmdb_unavailable")
            {
                var requested = error.Arguments.FirstOrDefault() as TimeSpan?;
                if (requested > TimeSpan.FromSeconds(5)) throw;
                var backoff = TimeSpan.FromMilliseconds(500 * (attempt + 1));
                await Task.Delay(requested > backoff ? requested.Value : backoff, ct);
            }
        }
    }

    private static string OperationId(WatchListItem item)
    {
        var key = string.Join("/", item.IsAnime ? "anime" : item.MediaCode,
            item.Year.ToString(CultureInfo.InvariantCulture), WatchList.TitleKey(item.Title));
        return "watch-list:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
    }

    private static int? SourceId(Work work) => work.MediaCode switch
    {
        "filme" => work.TmdbMovieId,
        "serie" => work.TmdbSeriesId,
        _ => null
    };

    private sealed record Match(string? Media, int? ExternalId, string Status);
    private sealed record Receipt(string WorkId, string Media, int? ExternalId, string Status);
}
