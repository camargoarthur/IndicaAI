using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IndicaAI.Web.Features.Batch;

public record BatchItem(string TrackingId, long ExpectedVersion);
public sealed class BatchService(DataPaths paths, IInterceptor[]? interceptors = null)
{
    private readonly CommandRunner runner = new(paths, interceptors);
    public Task<TrackingResult[]> ChangeStatesAsync(string operationId, IReadOnlyList<BatchItem> items, IReadOnlyDictionary<string, string> targets, bool confirmKeepCycle = false, CancellationToken ct = default) => runner.RunAsync(operationId, "batch.states", new { items, targets, confirmKeepCycle }, async (db, owner) =>
    {
        if (items.Count == 0 || items.Select(i => i.TrackingId).Distinct().Count() != items.Count) throw new DomainException("nothing_selected");
        var service = new TrackingService(paths); var result = new List<TrackingResult>();
        foreach (var item in items)
        {
            var t = await TrackingService.FindOwned(db, owner, item.TrackingId, ct); if (!targets.TryGetValue(t.MediaCode, out var target) || string.IsNullOrEmpty(target)) throw new DomainException("state_group_missing"); TrackingService.ValidateState(target); var cycle = await TrackingService.CurrentCycle(db, t.Id, ct);
            result.Add(await service.ChangeCoreAsync(db, owner, new(new(operationId, item.ExpectedVersion), t.Id, target, new(cycle.StartOn, cycle.CompletionOn, true), ConfirmKeepCycle: confirmKeepCycle), ct));
        }
        return result.ToArray();
    }, ct);
    public Task<int> ChangeListsAsync(string operationId, IReadOnlyList<BatchItem> items, string listId, bool add, CancellationToken ct = default) => runner.RunAsync(operationId, "batch.lists", new { items, listId, add }, async (db, owner) =>
    {
        if (items.Count == 0) throw new DomainException("nothing_selected");
        if (!db.Lists.Any(l => l.Id == listId && l.IdentityId == owner)) throw new DomainException("not_found");
        foreach (var item in items.DistinctBy(i => i.TrackingId)) { var t = await TrackingService.FindOwned(db, owner, item.TrackingId, ct); TrackingService.CheckVersion(t, item.ExpectedVersion); if (t.ArchivedAtUtc != null) throw new DomainException("archived"); var member = await db.ListItems.FindAsync([listId, t.Id], ct); if (add && member == null) { db.ListItems.Add(new() { IdentityId = owner, ListId = listId, TrackingId = t.Id }); t.Version++; } else if (!add && member != null) { db.ListItems.Remove(member); t.Version++; } }
        return items.Count;
    }, ct);
}
