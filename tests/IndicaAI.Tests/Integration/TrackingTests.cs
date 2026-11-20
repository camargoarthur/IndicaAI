using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public sealed class TrackingTests
{
    private static DateInput Empty => new(null, null, false);
    [Fact]
    public async Task RepeatAddAndOperationDoNotDuplicateTrackingOrEvents()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var service = new TrackingService(f.Paths);
        var command = new AddTracking("add-1", "cidade-distante", "in_progress", Empty);
        var first = await service.AddAsync(command);
        Assert.Equal(first, await service.AddAsync(command));
        var second = await service.AddAsync(command with { OperationId = "add-2" });
        Assert.Equal(first.TrackingId, second.TrackingId);
        await using var db = f.Open();
        Assert.Equal(1, await db.Trackings.CountAsync()); Assert.Equal(1, await db.Events.CountAsync());
        await Assert.ThrowsAsync<DomainException>(() => service.AddAsync(command with { StateCode = "completed" }));
    }
    [Fact]
    public async Task StateRequiredAndKnownAndVersionCannotBeOverwritten()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var s = new TrackingService(f.Paths);
        await Assert.ThrowsAsync<DomainException>(() => s.AddAsync(new("empty", "cidade-distante", "", Empty)));
        await Assert.ThrowsAsync<DomainException>(() => s.AddAsync(new("bad", "cidade-distante", "watching", Empty)));
        var t = await s.AddAsync(new("ok", "cidade-distante", "planned", Empty));
        var updated = await s.ChangeStateAsync(new(new("state", t.Version), t.TrackingId, "completed", Empty));
        await Assert.ThrowsAsync<DomainException>(() => s.ChangeStateAsync(new(new("stale", t.Version), t.TrackingId, "paused", Empty)));
        var unchanged = await s.ChangeStateAsync(new(new("same", updated.Version), t.TrackingId, "completed", Empty));
        Assert.Equal(updated.Version, unchanged.Version);
        await using var db = f.Open(); Assert.Equal(2, await db.Events.CountAsync());
    }
    [Fact]
    public async Task NewCycleAndDateCorrectionKeepOldCycleAndOriginalInclusion()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var s = new TrackingService(f.Paths);
        var first = await s.AddAsync(new("add", "cidade-distante", "completed", new(new(2020, 1, 1), new(2020, 2, 1), false)));
        var replay = await s.StartCycleAsync(new(new("cycle", first.Version), first.TrackingId, "completed", Empty));
        Assert.Equal(2, replay.CycleOrdinal);
        await using var db = f.Open();
        var cycles = await db.Cycles.OrderBy(c => c.Ordinal).ToListAsync();
        Assert.Equal(new DateOnly(2020, 1, 1), cycles[0].StartOn); Assert.Null(cycles[1].StartOn);
        var included = (await db.Trackings.SingleAsync()).IncludedOn;
        var corrected = await s.CorrectDateAsync(new(new("date", replay.Version), first.TrackingId, cycles[1].Id, "start", new(2021, 4, 2), false));
        await s.CorrectDateAsync(new(new("date2", corrected.Version), first.TrackingId, cycles[1].Id, "start", new(2021, 3, 1), false));
        await using var fresh = f.Open();
        Assert.Equal(included, (await fresh.Trackings.SingleAsync()).IncludedOn);
        Assert.Contains(await fresh.Events.ToListAsync(), e => e.OldValue == "2021-04-02" && e.NewValue == "2021-03-01");
        Assert.Equal("completed", (await fresh.Trackings.SingleAsync()).StateCode);
        Assert.Equal(4, await fresh.Events.CountAsync());
    }
    [Fact]
    public async Task UnusualDatesNeedConfirmationAndNoteIsPrivate()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var s = new TrackingService(f.Paths);
        var dates = new DateInput(new(2020, 2, 1), new(2020, 1, 1), false);
        await Assert.ThrowsAsync<DomainException>(() => s.AddAsync(new("bad-date", "cidade-distante", "in_progress", dates)));
        var t = await s.AddAsync(new("date-ok", "cidade-distante", "in_progress", dates with { ConfirmUnusualDates = true }));
        await s.SaveNoteAsync(new(new("note", t.Version), t.TrackingId, "<script>texto privado</script>"));
        await using var db = f.Open();
        Assert.Equal("<script>texto privado</script>", (await db.Trackings.SingleAsync()).Note);
        Assert.DoesNotContain("texto privado", (await db.Works.SingleAsync(w => w.Id == "cidade-distante")).Description!);
    }
}
