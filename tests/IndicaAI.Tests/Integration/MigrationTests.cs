using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace IndicaAI.Tests.Integration;

public class MigrationTests
{
    [Fact]
    public async Task MovieSourceMigrationPreservesExistingWorksAndTrackingAndEnforcesUniqueSource()
    {
        var f = new TestDatabase();
        await f.InitializeAsync();
        var tracking = await new TrackingService(f.Paths).AddAsync(new("tmdb-migration", "mare-de-estrelas", "planned", new(null, null, false), "manter"));
        await using (var before = f.Open())
            await before.GetService<IMigrator>().MigrateAsync("20261108205509_ImportIgnoredCount");

        await f.InitializeAsync();
        await using var after = f.Open();
        Assert.Null((await after.Works.SingleAsync(w => w.Id == "mare-de-estrelas")).TmdbMovieId);
        Assert.Equal("manter", (await after.Trackings.SingleAsync(t => t.Id == tracking.TrackingId)).Note);
        Assert.Single(await after.Events.ToArrayAsync());
        Assert.Single(Directory.GetDirectories(f.Paths.Backups, "b-u-*"));
        after.Works.Add(new() { Id = "source-first", Title = "Filme", MediaCode = "filme", TmdbMovieId = 426063 });
        await after.SaveChangesAsync();
        after.Works.Add(new() { Id = "source-duplicate", Title = "Outra obra", MediaCode = "filme", TmdbMovieId = 426063 });
        await Assert.ThrowsAsync<DbUpdateException>(() => after.SaveChangesAsync());
    }

    [Fact]
    public async Task FailedMigrationKeepsOldDataAndVerifiedPreMigrationBundle()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tracking = await new TrackingService(f.Paths).AddAsync(new("a", "cidade-distante", "planned", new(null, null, false), "preservar")); string[] before;
        await using (var db = f.Open()) { await db.GetService<IMigrator>().MigrateAsync("20261108174224_InitialLocalSchema"); await db.Database.ExecuteSqlRawAsync("UPDATE Credits SET Position=0 WHERE WorkId='cidade-distante'"); before = (await db.Database.GetAppliedMigrationsAsync()).ToArray(); }
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(() => f.InitializeAsync());
        await using (var db = f.Open()) { Assert.Equal(before, (await db.Database.GetAppliedMigrationsAsync()).ToArray()); Assert.Equal("preservar", (await db.Trackings.SingleAsync()).Note); Assert.Equal(tracking.TrackingId, (await db.Trackings.SingleAsync()).Id); Assert.True(await db.Credits.AnyAsync(c => c.WorkId == "cidade-distante" && c.Position == 0)); }
        var bundle = Assert.Single(Directory.GetDirectories(f.Paths.Backups, "b-u-*")); await new RestoreService(f.Paths).ValidateAsync(bundle);
    }
}
