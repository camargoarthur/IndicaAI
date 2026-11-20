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
    public async Task FailedMigrationKeepsOldDataAndVerifiedPreMigrationBundle()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tracking = await new TrackingService(f.Paths).AddAsync(new("a", "cidade-distante", "planned", new(null, null, false), "preservar")); string[] before;
        await using (var db = f.Open()) { await db.GetService<IMigrator>().MigrateAsync("20261108174224_InitialLocalSchema"); await db.Database.ExecuteSqlRawAsync("UPDATE Credits SET Position=0 WHERE WorkId='cidade-distante'"); before = (await db.Database.GetAppliedMigrationsAsync()).ToArray(); }
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(() => f.InitializeAsync());
        await using (var db = f.Open()) { Assert.Equal(before, (await db.Database.GetAppliedMigrationsAsync()).ToArray()); Assert.Equal("preservar", (await db.Trackings.SingleAsync()).Note); Assert.Equal(tracking.TrackingId, (await db.Trackings.SingleAsync()).Id); Assert.True(await db.Credits.AnyAsync(c => c.WorkId == "cidade-distante" && c.Position == 0)); }
        var bundle = Assert.Single(Directory.GetDirectories(f.Paths.Backups, "b-u-*")); await new RestoreService(f.Paths).ValidateAsync(bundle);
    }
}
