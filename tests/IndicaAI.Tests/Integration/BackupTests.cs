using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Operations;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public class BackupTests
{
    [Fact]
    public async Task RestorePreservesSnapshotAndCorruptBundleKeepsActive()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tracking = new TrackingService(f.Paths); var t = await tracking.AddAsync(new("a", "cidade-distante", "in_progress", new(new(2020, 1, 2), null, false), "snapshot")); var list = await new ListService(f.Paths).SaveAsync("l", null, 0, "Mista"); await new ListService(f.Paths).SetMembershipAsync(new("m", t.Version), t.TrackingId, list.Id, true);
        var backup = await new BackupService(f.Paths).CreateAsync("manual"); await tracking.SaveNoteAsync(new(new("note", 2), t.TrackingId, "later"));
        var restore = new RestoreService(f.Paths); await restore.ValidateAsync(backup); await restore.RestoreAsync(backup);
        await using (var db = f.Open()) { Assert.Equal("snapshot", (await db.Trackings.SingleAsync()).Note); Assert.Equal(t.TrackingId, (await db.ListItems.SingleAsync()).TrackingId); Assert.Single(await db.Events.ToListAsync()); Assert.Equal(new DateOnly(2020, 1, 2), (await db.Cycles.SingleAsync()).StartOn); }
        var before = await File.ReadAllBytesAsync(f.Paths.Database); await File.AppendAllTextAsync(Path.Combine(backup, "data", "indicaai.db"), "corrupt"); await Assert.ThrowsAsync<DomainException>(() => restore.RestoreAsync(backup)); Assert.Equal(before, await File.ReadAllBytesAsync(f.Paths.Database));
    }
    [Fact]
    public async Task InstanceLockAndUnsafeManifestAreRejected()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); using (var instance = LocalInstance.Acquire(f.Paths)) { Assert.Throws<DomainException>(() => LocalInstance.Acquire(f.Paths)); await Assert.ThrowsAsync<DomainException>(() => new RestoreService(f.Paths).RestoreAsync("missing")); }
        var bundle = await new BackupService(f.Paths).CreateAsync("manual"); var manifest = Path.Combine(bundle, "manifest.json"); var text = await File.ReadAllTextAsync(manifest); await File.WriteAllTextAsync(manifest, text.Replace("data/indicaai.db", "../escape.db")); await Assert.ThrowsAsync<DomainException>(() => new RestoreService(f.Paths).ValidateAsync(bundle));
    }
}
