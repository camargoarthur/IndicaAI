using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Catalog;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using System.Text;
namespace IndicaAI.Web.Operations;

public static class MaintenanceCommand
{
    public static async Task<int> RunAsync(string[] args, CancellationToken ct = default)
    {
        try
        {
            string? Value(string key) { var index = Array.IndexOf(args, key); return index >= 0 && index + 1 < args.Length ? args[index + 1] : null; }
            var root = Value("--data-root") ?? Environment.GetEnvironmentVariable("INDICAAI_DATA_ROOT") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IndicaAI"); var paths = DataPaths.ForRoot(root); var action = Value("--maintenance");
            if (action == "backup") { using var instance = LocalInstance.Acquire(paths); Console.WriteLine(await new BackupService(paths).CreateAsync("manual", ct)); }
            else if (action == "validate") { await new RestoreService(paths).ValidateAsync(Value("--bundle") ?? throw new DomainException("backup_invalid"), ct); Console.WriteLine("OK"); }
            else if (action == "restore") { await new RestoreService(paths).RestoreAsync(Value("--bundle") ?? throw new DomainException("backup_invalid"), ct); Console.WriteLine("OK"); }
            else if (action == "import-watch-list") return await ImportWatchListAsync(args, Value("--data-root"), Value("--list"), ct);
            else throw new DomainException("backup_invalid"); return 0;
        }
        catch (DomainException e)
        {
            Console.Error.WriteLine(e.Code == "watch_list_invalid" && e.Arguments is [int line] ? $"{e.Code}: line {line}" : e.Code);
            return 2;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or DecoderFallbackException or Microsoft.Data.Sqlite.SqliteException) { Console.Error.WriteLine("operation_failed: " + e.GetType().Name); return 3; }
    }

    private static async Task<int> ImportWatchListAsync(string[] args, string? root, string? list, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(root) || root.StartsWith("--") || string.IsNullOrWhiteSpace(list) || list.StartsWith("--"))
            throw new DomainException("watch_list_arguments");
        var preview = args.Contains("--preview", StringComparer.Ordinal);
        var skipPosters = args.Contains("--skip-posters", StringComparer.Ordinal);
        var postersOnly = args.Contains("--posters-only", StringComparer.Ordinal);
        if (postersOnly && (preview || skipPosters)) throw new DomainException("watch_list_arguments");

        if (new FileInfo(list).Length > 256 * 1024) throw new DomainException("watch_list_invalid");
        using var input = new StreamReader(list, new UTF8Encoding(false, true));
        var data = WatchList.Read(input);
        var movieLines = AnimeLines("--anime-film-lines");
        var seriesLines = AnimeLines("--anime-series-lines");
        if (movieLines.Overlaps(seriesLines)) throw new DomainException("watch_list_arguments");
        var items = data.Select(item => movieLines.Contains(item.Line) ? item with { MediaCode = "filme" }
            : seriesLines.Contains(item.Line) ? item with { MediaCode = "serie" } : item).ToArray();
        Console.WriteLine($"entries={items.Length}; films={items.Count(i => i.MediaCode == "filme")}; "
            + $"series={items.Count(i => i.MediaCode == "serie")}; anime={items.Count(i => i.IsAnime)}");
        if (preview) return 0;

        if (!postersOnly && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("INDICAAI_TMDB_READ_TOKEN")))
            throw new DomainException("tmdb_configuration");
        var paths = DataPaths.ForRoot(root);
        if (postersOnly && !File.Exists(paths.Database)) throw new DomainException("watch_list_database_missing");
        using var instance = LocalInstance.Acquire(paths);
        if (!postersOnly)
            await new DatabaseBootstrapper(paths, demoCatalog: args.Contains("--demo-catalog", StringComparer.Ordinal)).InitializeAsync(ct);
        using var services = new ServiceCollection().AddHttpClient("Tmdb", client =>
        {
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            client.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
            .RedactLoggedHeaders(["Authorization"]).Services.BuildServiceProvider();
        using var imageClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(4) };
        var tmdb = new TmdbMovies(services.GetRequiredService<IHttpClientFactory>(), new ConfigurationBuilder().AddEnvironmentVariables().Build());
        var importer = new WatchListImporter(paths, tmdb, new TmdbPosterCache(paths, imageClient), skipPosters);
        var results = postersOnly ? await importer.ResumePostersAsync(items, ct)
            : await importer.RunAsync(items, result => Console.WriteLine($"line {result.Line}: {result.Status}"), ct);
        var posterResults = results.Where(result => result.PosterStatus is not null).ToArray();
        foreach (var result in posterResults)
        {
            var work = result.WorkId is { } id ? $"; work={id}" : "";
            Console.WriteLine($"line {result.Line}: poster={result.PosterStatus}{work}");
        }
        if (postersOnly)
        {
            var works = posterResults.Where(result => result.WorkId is not null).DistinctBy(result => result.WorkId).ToArray();
            var retry = works.Count(result => result.PosterStatus is "download_failed" or "tmdb_unavailable"
                or "tmdb_rate_limit" or "tmdb_configuration" or "tmdb_authentication"
                || result.PosterStatus!.StartsWith("deferred_", StringComparison.Ordinal));
            Console.WriteLine($"posters_downloaded={works.Count(result => result.PosterStatus == "downloaded")}; "
                + $"posters_retry={retry}");
            return retry == 0 ? 0 : 4;
        }

        var saved = results.Count(result => result.WorkId is not null);
        Console.WriteLine($"saved={saved}/{items.Length}; linked={results.Count(result => result.Status == "linked")}; "
            + $"local={results.Count(result => result.Status is "local_ambiguous" or "local_not_found")}; pending={items.Length - saved}");
        return saved == items.Length ? 0 : 4;

        HashSet<int> AnimeLines(string option)
        {
            var index = Array.IndexOf(args, option);
            if (index < 0) return [];
            if (index + 1 >= args.Length) throw new DomainException("watch_list_arguments");
            var ids = new HashSet<int>();
            foreach (var part in args[index + 1].Split(','))
            {
                if (!int.TryParse(part, out var number) || !data.Any(item => item.IsAnime && item.Line == number))
                    throw new DomainException("watch_list_arguments");
                ids.Add(number);
            }
            return ids;
        }
    }
}
