using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Web.Features.Catalog;

public sealed class TmdbCatalog(DataPaths paths)
{
    public async Task<string?> FindMovieAsync(int tmdbId, CancellationToken ct)
    {
        await using var db = new AppDbContext(paths);
        return await MovieId(db, tmdbId, ct);
    }

    public async Task<string?> FindSeriesAsync(int tmdbId, CancellationToken ct)
    {
        await using var db = new AppDbContext(paths);
        return await BySource(db, "serie", tmdbId).Select(work => work.Id).SingleOrDefaultAsync(ct);
    }

    public async Task<string> ImportSeriesAsync(TmdbSeries series, bool isAnime, CancellationToken ct)
    {
        var gate = DatabaseGate.For(paths);
        await gate.WaitAsync(ct);
        try
        {
            await using var db = new AppDbContext(paths);
            if (await BySource(db, "serie", series.Id).SingleOrDefaultAsync(ct) is { } existing) return existing.Id;
            var work = new Work
            {
                Id = Guid.NewGuid().ToString(),
                MediaCode = "serie",
                TmdbSeriesId = series.Id,
                Title = series.Title,
                ReleaseYear = series.Year,
                IsAnime = isAnime
            };
            db.Works.Add(work);
            db.Credits.AddRange(series.Creators.Select((name, index) => new WorkCredit
            {
                WorkId = work.Id, Role = "creator", Position = index + 1, Name = name
            }));
            await db.SaveChangesAsync(ct);
            return work.Id;
        }
        finally { gate.Release(); }
    }

    public async Task<string> ImportMovieAsync(TmdbMovie movie, bool isAnime, CancellationToken ct)
    {
        var gate = DatabaseGate.For(paths);
        await gate.WaitAsync(ct);
        try
        {
            await using var db = new AppDbContext(paths);
            if (await MovieId(db, movie.Id, ct) is { } existing) return existing;

            var work = new Work
            {
                Id = Guid.NewGuid().ToString(),
                MediaCode = "filme",
                TmdbMovieId = movie.Id,
                Title = movie.Title,
                ReleaseYear = movie.Year,
                IsAnime = isAnime
            };
            db.Works.Add(work);
            if (movie.Minutes is int duration) db.Films.Add(new() { WorkId = work.Id, DurationMinutes = duration });
            db.Credits.AddRange(movie.Directors.Select((name, index) => new WorkCredit
            {
                WorkId = work.Id, Role = "director", Position = index + 1, Name = name
            }));
            await db.SaveChangesAsync(ct);
            return work.Id;
        }
        finally { gate.Release(); }
    }

    private static Task<string?> MovieId(AppDbContext db, int tmdbId, CancellationToken ct) =>
        BySource(db, "filme", tmdbId).Select(work => work.Id).SingleOrDefaultAsync(ct);

    internal static IQueryable<Work> BySource(AppDbContext db, string media, int tmdbId) => media switch
    {
        "filme" => db.Works.Where(work => work.MediaCode == "filme" && work.TmdbMovieId == tmdbId),
        "serie" => db.Works.Where(work => work.MediaCode == "serie" && work.TmdbSeriesId == tmdbId),
        _ => throw new ArgumentOutOfRangeException(nameof(media))
    };
}
