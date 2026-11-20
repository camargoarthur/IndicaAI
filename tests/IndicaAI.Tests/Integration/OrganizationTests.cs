using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Features.Archive;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Integration;

public class OrganizationTests
{
    [Fact]
    public async Task MixedListAndArchiveRestorePreserveAllPersonalData()
    {
        var f = new TestDatabase(); await f.InitializeAsync();
        var tracking = new TrackingService(f.Paths); var lists = new ListService(f.Paths); var archive = new ArchiveService(f.Paths);
        await using var db = f.Open(); var works = await db.Works.OrderBy(w => w.Id).Take(2).ToArrayAsync();
        var a = await tracking.AddAsync(new("oa", works[0].Id, "in_progress", new(null, null, false), "privada"));
        var b = await tracking.AddAsync(new("ob", works[1].Id, "planned", new(null, null, false)));
        var list = await lists.SaveAsync("list", null, 0, "Fim de semana");
        await lists.SetMembershipAsync(new("ma", a.Version), a.TrackingId, list.Id, true);
        await lists.SetMembershipAsync(new("mb", b.Version), b.TrackingId, list.Id, true);
        var changed = await archive.SetAsync(new("arc", 2), a.TrackingId, true);
        await archive.SetAsync(new("restore", changed.Version), a.TrackingId, false);
        await archive.SetAsync(new("restore", changed.Version), a.TrackingId, false);
        await using var read = f.Open(); var restored = await read.Trackings.FindAsync(a.TrackingId);
        Assert.Null(restored!.ArchivedAtUtc); Assert.Equal("privada", restored.Note); Assert.Equal("in_progress", restored.StateCode);
        Assert.Equal(2, await read.ListItems.CountAsync()); Assert.Single(await read.Events.Where(e => e.TrackingId == a.TrackingId).ToArrayAsync());
        await Assert.ThrowsAsync<DomainException>(() => lists.SetMembershipAsync(new("stale", 1), a.TrackingId, list.Id, false));
        await Assert.ThrowsAsync<DomainException>(() => lists.SetMembershipAsync(new("bad", restored.Version), a.TrackingId, "missing", true));
    }
}
