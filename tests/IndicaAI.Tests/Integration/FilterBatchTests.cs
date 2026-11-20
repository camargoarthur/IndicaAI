using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Features.Library;
using IndicaAI.Web.Features.Batch;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public class FilterBatchTests
{
    [Fact]
    public async Task AllAnyNotHonorContextAndBatchIsAtomic()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); await using var db = f.Open();
        var works = await db.Works.OrderBy(w => w.Id).Take(3).ToArrayAsync(); var ts = new TrackingService(f.Paths);
        var items = new List<BatchItem>(); foreach (var w in works) { var r = await ts.AddAsync(new(Guid.NewGuid().ToString(), w.Id, "planned", new(null, null, false))); items.Add(new(r.TrackingId, r.Version)); }
        var owner = (await db.Identities.SingleAsync()).Id;
        db.Lists.AddRange(new PersonalList { Id = "terror", Name = "Terror", IdentityId = owner }, new PersonalList { Id = "space", Name = "Espacial", IdentityId = owner }, new PersonalList { Id = "humor", Name = "Humor", IdentityId = owner });
        foreach (var (i, l) in new[] { (0, "terror"), (0, "space"), (1, "terror"), (2, "space"), (2, "humor") }) db.ListItems.Add(new() { IdentityId = owner, TrackingId = items[i].TrackingId, ListId = l }); await db.SaveChangesAsync();
        var q = new LibraryQuery(f.Paths); var filter = new LibraryFilter { Include = [new("list", "terror"), new("list", "space")] };
        Assert.Equal(new[] { works[0].Id }, (await q.ExecuteAsync(filter)).Items.Select(x => x.Work.Id));
        filter = filter with { Mode = "any" }; Assert.Equal(3, (await q.ExecuteAsync(filter)).Total);
        filter = filter with { Exclude = [new("list", "humor")] }; Assert.Equal(2, (await q.ExecuteAsync(filter)).Total);
        Assert.Equal(0, (await q.ExecuteAsync(filter with { Context = "archived" })).Total);
        var batch = new BatchService(f.Paths); var targets = works.Take(2).ToDictionary(w => w.MediaCode, _ => "completed");
        await Assert.ThrowsAsync<DomainException>(() => batch.ChangeStatesAsync("missing", items, targets));
        Assert.Equal(3, await db.Trackings.CountAsync(t => t.StateCode == "planned"));
        targets = works.ToDictionary(w => w.MediaCode, _ => "completed");
        await Assert.ThrowsAsync<DomainException>(() => batch.ChangeStatesAsync("stale", [items[0], items[1] with { ExpectedVersion = 0 }, items[2]], targets));
        await batch.ChangeStatesAsync("ok", items, targets); await batch.ChangeStatesAsync("ok", items, targets);
        await using var read = f.Open(); Assert.Equal(3, await read.Trackings.CountAsync(t => t.StateCode == "completed")); Assert.Equal(6, await read.Events.CountAsync());
    }
}
