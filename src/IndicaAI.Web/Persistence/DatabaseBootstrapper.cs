using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using IndicaAI.Web.Operations;

namespace IndicaAI.Web.Persistence;

public sealed class DatabaseBootstrapper(DataPaths paths)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await RestoreService.RecoverPendingAsync(paths,ct);
        paths.EnsureDirectories();
        var existed=File.Exists(paths.Database);
        await using var db = new AppDbContext(paths);
        if(existed)
        {
            var applied=(await db.Database.GetAppliedMigrationsAsync(ct)).ToArray();
            var known=db.Database.GetMigrations().ToArray();
            if(applied.Length==0||applied.Any(id=>!known.Contains(id)))throw new DomainException("backup_invalid");
            if((await db.Database.GetPendingMigrationsAsync(ct)).Any())await new BackupService(paths).CreateAsync("pre-migration",ct);
            await new BackupService(paths).DailyAsync(ct);
        }
        await db.Database.MigrateAsync(ct);
        BundledAssets.Ensure(paths);
        if (await db.Identities.AnyAsync(ct)) return;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var id = Guid.NewGuid().ToString();
        db.Identities.Add(new() { Id = id, CreatedAtUtc = DateTime.UtcNow.ToString("O") });
        db.Preferences.Add(new() { IdentityId = id });
        foreach (var media in MediaCodes.All)
        {
            db.Media.Add(new() { Code = media });
            foreach (var code in MediaCodes.States) db.BaseStates.Add(new() { MediaCode = media, Code = code });
        }
        db.Works.AddRange(
            new Work { Id = "alem-do-vale", MediaCode = "jogo", Title = "Além do vale", Description = "Uma jornada por vales e ruínas que guardam a memória de um mundo antigo.", CoverAsset = "alem-do-vale.png", ReleaseYear = 2026 },
            new Work { Id = "cidade-distante", MediaCode = "livro", Title = "A cidade distante", Description = "Entre ruas antigas e lembranças, uma cidade revela os caminhos de uma nova história.", CoverAsset = "cidade-distante.png", ReleaseYear = 2026 },
            new Work { Id = "jardins-de-inverno", MediaCode = "manga", Title = "Jardins de inverno", Description = "Flores, silêncios e encontros inesperados em uma estação de mudanças.", CoverAsset = "jardins-de-inverno.png", ReleaseYear = 2026 },
            new Work { Id = "mare-de-estrelas", MediaCode = "filme", Title = "Maré de estrelas", Description = "Uma noite à beira-mar, um farol e duas pessoas à procura de novas constelações.", CoverAsset = "mare-de-estrelas.png", ReleaseYear = 2026 },
            new Work { Id = "rota-do-norte", MediaCode = "serie", Title = "Rota do norte", Description = "Um trem atravessa paisagens de inverno e aproxima histórias que pareciam distantes.", IsAnime = true, CoverAsset = "rota-do-norte.png", ReleaseYear = 2026 });
        db.Games.Add(new(){WorkId="alem-do-vale",Studio="Estúdio Horizonte"});
        db.Platforms.Add(new(){WorkId="alem-do-vale",Name="PC",NameKey="pc"});
        db.Films.Add(new(){WorkId="mare-de-estrelas",DurationMinutes=110});
        db.Credits.AddRange(new WorkCredit{WorkId="cidade-distante",Role="author",Position=1,Name="Clara Duarte"},new WorkCredit{WorkId="jardins-de-inverno",Role="author",Position=1,Name="Aki Mori"},new WorkCredit{WorkId="mare-de-estrelas",Role="director",Position=1,Name="Lia Costa"},new WorkCredit{WorkId="rota-do-norte",Role="creator",Position=1,Name="Haru Watanabe"});
        db.Revisions.Add(new());
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}

public sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => new(DataPaths.ForRoot(Path.Combine(Path.GetTempPath(), "IndicaAI-design-only")));
}
