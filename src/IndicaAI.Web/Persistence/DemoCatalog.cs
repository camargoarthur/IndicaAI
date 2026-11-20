using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Operations;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Web.Persistence;

internal static class DemoCatalog
{
    private sealed record Example(string Id, string Media, string Title, string Description,
        bool Anime = false, string? CreditRole = null, string? CreditName = null);

    private static readonly Example[] Examples =
    [
        new("alem-do-vale", "jogo", "Além do vale", "Uma jornada por vales e ruínas que guardam a memória de um mundo antigo."),
        new("cidade-distante", "livro", "A cidade distante", "Entre ruas antigas e lembranças, uma cidade revela os caminhos de uma nova história.", CreditRole: "author", CreditName: "Clara Duarte"),
        new("jardins-de-inverno", "manga", "Jardins de inverno", "Flores, silêncios e encontros inesperados em uma estação de mudanças.", CreditRole: "author", CreditName: "Aki Mori"),
        new("mare-de-estrelas", "filme", "Maré de estrelas", "Uma noite à beira-mar, um farol e duas pessoas à procura de novas constelações.", CreditRole: "director", CreditName: "Lia Costa"),
        new("rota-do-norte", "serie", "Rota do norte", "Um trem atravessa paisagens de inverno e aproxima histórias que pareciam distantes.", true, "creator", "Haru Watanabe")
    ];

    public static void Add(AppDbContext db)
    {
        foreach (var example in Examples)
        {
            db.Works.Add(new Work { Id = example.Id, MediaCode = example.Media, Title = example.Title,
                Description = example.Description, IsAnime = example.Anime, CoverAsset = example.Id + ".png", ReleaseYear = 2026 });
            if (example.CreditRole is { } role)
                db.Credits.Add(new WorkCredit { WorkId = example.Id, Role = role, Position = 1, Name = example.CreditName! });
        }
        db.Games.Add(new() { WorkId = "alem-do-vale", Studio = "Estúdio Horizonte" });
        db.Platforms.Add(new() { WorkId = "alem-do-vale", Name = "PC", NameKey = "pc" });
        db.Films.Add(new() { WorkId = "mare-de-estrelas", DurationMinutes = 110 });
    }

    public static async Task RemoveUnusedAsync(AppDbContext db, DataPaths paths, CancellationToken ct)
    {
        var gate = DatabaseGate.For(paths);
        await gate.WaitAsync(ct);
        try
        {
            foreach (var example in Examples)
            {
                var work = await db.Works.FindAsync([example.Id], ct);
                if (work is null || work.Title != example.Title || work.Description != example.Description
                    || work.MediaCode != example.Media || work.IsAnime != example.Anime || work.Version != 1
                    || work.CoverAsset != example.Id + ".png" || work.ReleaseYear != 2026
                    || work.TmdbMovieId is not null || work.TmdbSeriesId is not null
                    || await db.Trackings.AnyAsync(tracking => tracking.WorkId == work.Id, ct)
                    || await db.WorkTags.AnyAsync(tag => tag.WorkId == work.Id, ct)) continue;

                var credits = await db.Credits.Where(credit => credit.WorkId == work.Id).ToArrayAsync(ct);
                if (example.CreditRole is null ? credits.Length != 0 : credits.Length != 1
                    || credits[0].Role != example.CreditRole || credits[0].Name != example.CreditName || credits[0].Position != 1) continue;

                var game = await db.Games.FindAsync([work.Id], ct);
                var platforms = await db.Platforms.Where(platform => platform.WorkId == work.Id).ToArrayAsync(ct);
                var film = await db.Films.FindAsync([work.Id], ct);
                if (example.Media == "jogo" ? game?.Studio != "Estúdio Horizonte" || platforms.Length != 1
                    || platforms[0].Name != "PC" || platforms[0].NameKey != "pc" : game is not null || platforms.Length != 0) continue;
                if (example.Media == "filme" ? film?.DurationMinutes != 110 : film is not null) continue;

                db.Credits.RemoveRange(credits);
                db.Platforms.RemoveRange(platforms);
                if (game is not null) db.Games.Remove(game);
                if (film is not null) db.Films.Remove(film);
                db.Works.Remove(work);
            }
            if (!db.ChangeTracker.HasChanges()) return;
            await new BackupService(paths).CreateCoreAsync("pre-demo-cleanup", ct);
            await db.SaveChangesAsync(ct);
        }
        finally { gate.Release(); }
    }
}
