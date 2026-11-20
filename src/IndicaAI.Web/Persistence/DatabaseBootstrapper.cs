using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using IndicaAI.Web.Operations;

namespace IndicaAI.Web.Persistence;

public sealed class DatabaseBootstrapper(DataPaths paths, bool demoCatalog = false)
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
        if (demoCatalog) BundledAssets.Ensure(paths);
        if (await db.Identities.AnyAsync(ct))
        {
            if (!demoCatalog) await DemoCatalog.RemoveUnusedAsync(db, paths, ct);
            return;
        }
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var id = Guid.NewGuid().ToString();
        db.Identities.Add(new() { Id = id, CreatedAtUtc = DateTime.UtcNow.ToString("O") });
        db.Preferences.Add(new() { IdentityId = id });
        foreach (var media in MediaCodes.All)
        {
            db.Media.Add(new() { Code = media });
            foreach (var code in MediaCodes.States) db.BaseStates.Add(new() { MediaCode = media, Code = code });
        }
        if (demoCatalog) DemoCatalog.Add(db);
        db.Revisions.Add(new());
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}

public sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => new(DataPaths.ForRoot(Path.Combine(Path.GetTempPath(), "IndicaAI-design-only")));
}
