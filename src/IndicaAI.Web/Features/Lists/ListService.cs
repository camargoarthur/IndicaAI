using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Lists;

public record ListResult(string Id, long Version);
public sealed class ListService(DataPaths paths)
{
    private readonly CommandRunner runner = new(paths);
    public Task<ListResult> SaveAsync(string operationId, string? id, long version, string name, CancellationToken ct = default) => runner.RunAsync(operationId, "list.save", new { id, version, name }, async (db, owner) =>
    {
        var value = name.Trim(); if (value.Length == 0) throw new DomainException("name_required");
        PersonalList list;
        if (id is null) { list = new() { Id = Guid.NewGuid().ToString(), IdentityId = owner, Name = value }; db.Lists.Add(list); }
        else { list = await db.Lists.SingleOrDefaultAsync(l => l.Id == id && l.IdentityId == owner, ct) ?? throw new DomainException("not_found"); if (list.Version != version) throw new DomainException("conflict"); if (list.Name != value) { list.Name = value; list.Version++; } }
        return new ListResult(list.Id, list.Version);
    }, ct);
    public Task<TrackingResult> SetMembershipAsync(Mutation mutation, string trackingId, string listId, bool add, CancellationToken ct = default) => runner.RunAsync(mutation.OperationId, "list.member", new { mutation, trackingId, listId, add }, async (db, owner) =>
    {
        var t = await TrackingService.FindOwned(db, owner, trackingId, ct); TrackingService.CheckVersion(t, mutation.ExpectedVersion);
        if (!await db.Lists.AnyAsync(l => l.Id == listId && l.IdentityId == owner, ct)) throw new DomainException("not_found");
        var item = await db.ListItems.SingleOrDefaultAsync(i => i.IdentityId == owner && i.ListId == listId && i.TrackingId == trackingId, ct);
        if (add && item is null) { db.ListItems.Add(new() { IdentityId = owner, ListId = listId, TrackingId = trackingId }); t.Version++; }
        if (!add && item is not null) { db.ListItems.Remove(item); t.Version++; }
        return new TrackingResult(t.Id, t.Version, t.StateCode, (await TrackingService.CurrentCycle(db, t.Id, ct)).Ordinal);
    }, ct);
}
