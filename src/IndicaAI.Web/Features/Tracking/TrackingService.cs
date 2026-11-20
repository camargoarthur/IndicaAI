using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IndicaAI.Web.Features.Tracking;

public sealed class TrackingService(DataPaths paths, TimeProvider? clock = null, IInterceptor[]? interceptors = null)
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    private readonly CommandRunner runner = new(paths, interceptors);
    private string Now => time.GetUtcNow().UtcDateTime.ToString("O");
    private DateOnly Today => DateOnly.FromDateTime(time.GetLocalNow().DateTime);

    public Task<TrackingResult> AddAsync(AddTracking command, CancellationToken ct = default) =>
        runner.RunAsync(command.OperationId, "tracking.add", command, async (db, owner) =>
        {
            var work = await db.Works.FindAsync([command.WorkId], ct) ?? throw new DomainException("not_found");
            ValidateState(command.StateCode); ValidateDates(command.Dates);
            var existing = await db.Trackings.SingleOrDefaultAsync(t => t.IdentityId == owner && t.WorkId == work.Id, ct);
            if (existing is not null)
            {
                if (existing.ArchivedAtUtc is not null) throw new DomainException("archived");
                return await Result(db, existing, ct);
            }
            var tracking = new Domain.Tracking { Id = Guid.NewGuid().ToString(), IdentityId = owner, WorkId = work.Id, MediaCode = work.MediaCode, StateCode = command.StateCode, IncludedAtUtc = Now, IncludedOn = Today, Note = command.Note };
            var cycle = new ConsumptionCycle { Id = Guid.NewGuid().ToString(), TrackingId = tracking.Id, Ordinal = 1, CreatedAtUtc = Now, StartOn = command.Dates.Start, CompletionOn = command.Dates.Completion };
            db.Trackings.Add(tracking); db.Cycles.Add(cycle);
            if (command.ListIds is not null)
            {
                var ids = command.ListIds.Distinct().ToArray();
                if (await db.Lists.CountAsync(l => ids.Contains(l.Id) && l.IdentityId == owner, ct) != ids.Length) throw new DomainException("not_found");
                foreach (var listId in ids) db.ListItems.Add(new() { IdentityId = owner, TrackingId = tracking.Id, ListId = listId });
            }
            await AddEvent(db, tracking, cycle, "added", null, command.StateCode, command.OperationId, ct);
            return new TrackingResult(tracking.Id, tracking.Version, tracking.StateCode, 1);
        }, ct);

    public Task<TrackingResult> ChangeStateAsync(ChangeState command, CancellationToken ct = default) =>
        runner.RunAsync(command.Mutation.OperationId, "tracking.change", command, (db, owner) => ChangeCoreAsync(db, owner, command, ct), ct);

    internal async Task<TrackingResult> ChangeCoreAsync(AppDbContext db, string owner, ChangeState command, CancellationToken ct)
    {
        var t = await FindOwned(db, owner, command.TrackingId, ct); CheckVersion(t, command.Mutation.ExpectedVersion);
        if (t.ArchivedAtUtc is not null) throw new DomainException("archived");
        ValidateState(command.StateCode); ValidateDates(command.Dates);
        if (t.StateCode == "completed" && command.StateCode == "in_progress" && !command.ConfirmKeepCycle) throw new DomainException("choose_cycle");
        var cycle = await CurrentCycle(db, t.Id, ct);
        bool changed = false;
        if (t.StateCode != command.StateCode)
        {
            await AddEvent(db, t, cycle, "state_changed", t.StateCode, command.StateCode, command.Mutation.OperationId, ct);
            t.StateCode = command.StateCode; changed = true;
        }
        changed |= await SetDate(db, t, cycle, "start", command.Dates.Start, command.Mutation.OperationId, ct);
        changed |= await SetDate(db, t, cycle, "completion", command.Dates.Completion, command.Mutation.OperationId, ct);
        if (command.Note is not null && command.Note != t.Note) { t.Note = command.Note; changed = true; }
        if (command.ListIds is not null)
        {
            var ids = command.ListIds.Distinct().ToArray();
            if (await db.Lists.CountAsync(l => ids.Contains(l.Id) && l.IdentityId == owner, ct) != ids.Length) throw new DomainException("not_found");
            var members = await db.ListItems.Where(i => i.TrackingId == t.Id).ToListAsync(ct);
            foreach (var item in members.Where(i => !ids.Contains(i.ListId))) { db.ListItems.Remove(item); changed = true; }
            foreach (var id in ids.Where(id => !members.Any(i => i.ListId == id))) { db.ListItems.Add(new() { IdentityId = owner, ListId = id, TrackingId = t.Id }); changed = true; }
        }
        if (changed) t.Version++;
        return new(t.Id, t.Version, t.StateCode, cycle.Ordinal);
    }

    public Task<TrackingResult> StartCycleAsync(StartCycle command, CancellationToken ct = default) =>
        runner.RunAsync(command.Mutation.OperationId, "tracking.cycle", command, async (db, owner) =>
        {
            var t = await FindOwned(db, owner, command.TrackingId, ct); CheckVersion(t, command.Mutation.ExpectedVersion);
            if (t.ArchivedAtUtc is not null) throw new DomainException("archived");
            ValidateState(command.StateCode); ValidateDates(command.Dates);
            var previous = await CurrentCycle(db, t.Id, ct);
            var cycle = new ConsumptionCycle { Id = Guid.NewGuid().ToString(), TrackingId = t.Id, Ordinal = previous.Ordinal + 1, CreatedAtUtc = Now, StartOn = command.Dates.Start, CompletionOn = command.Dates.Completion };
            db.Cycles.Add(cycle);
            await AddEvent(db, t, cycle, "new_cycle", t.StateCode, command.StateCode, command.Mutation.OperationId, ct);
            t.StateCode = command.StateCode; t.Version++;
            return new TrackingResult(t.Id, t.Version, t.StateCode, cycle.Ordinal);
        }, ct);

    public Task<TrackingResult> CorrectDateAsync(CorrectDate command, CancellationToken ct = default) =>
        runner.RunAsync(command.Mutation.OperationId, "tracking.date", command, async (db, owner) =>
        {
            var t = await FindOwned(db, owner, command.TrackingId, ct); CheckVersion(t, command.Mutation.ExpectedVersion);
            var cycle = await db.Cycles.SingleOrDefaultAsync(c => c.Id == command.CycleId && c.TrackingId == t.Id, ct) ?? throw new DomainException("not_found");
            if (command.Field is not ("start" or "completion")) throw new DomainException("invalid_date");
            ValidateDates(new(command.Field == "start" ? command.Value : cycle.StartOn, command.Field == "completion" ? command.Value : cycle.CompletionOn, command.ConfirmUnusualDates));
            if (await SetDate(db, t, cycle, command.Field, command.Value, command.Mutation.OperationId, ct)) t.Version++;
            return await Result(db, t, ct);
        }, ct);

    public Task<TrackingResult> SaveNoteAsync(SaveNote command, CancellationToken ct = default) =>
        runner.RunAsync(command.Mutation.OperationId, "tracking.note", command, async (db, owner) =>
        {
            var t = await FindOwned(db, owner, command.TrackingId, ct); CheckVersion(t, command.Mutation.ExpectedVersion);
            if (t.Note != command.Text) { t.Note = command.Text; t.Version++; }
            return await Result(db, t, ct);
        }, ct);

    private async Task<bool> SetDate(AppDbContext db, Domain.Tracking tracking, ConsumptionCycle cycle, string field, DateOnly? value, string operation, CancellationToken ct)
    {
        var old = field == "start" ? cycle.StartOn : cycle.CompletionOn;
        if (old == value) return false;
        var corrected = await db.Events.Where(e => e.TrackingId == tracking.Id && e.CycleId == cycle.Id && (e.Field == field || e.Type == "added" || e.Type == "new_cycle")).OrderByDescending(e => e.Sequence).FirstOrDefaultAsync(ct);
        var e = await AddEvent(db, tracking, cycle, "date_corrected", null, null, operation, ct);
        e.Field = field; e.OldValue = old?.ToString("yyyy-MM-dd"); e.NewValue = value?.ToString("yyyy-MM-dd"); e.CorrectedEventId = corrected?.Id;
        if (field == "start") cycle.StartOn = value; else cycle.CompletionOn = value;
        cycle.Version++;
        return true;
    }
    private async Task<TrackingEvent> AddEvent(AppDbContext db, Domain.Tracking t, ConsumptionCycle c, string type, string? from, string? to, string op, CancellationToken ct)
    {
        var max = await db.Events.Where(e => e.TrackingId == t.Id).Select(e => (int?)e.Sequence).MaxAsync(ct) ?? 0;
        max = Math.Max(max, db.Events.Local.Where(e => e.TrackingId == t.Id).Select(e => e.Sequence).DefaultIfEmpty().Max());
        var e = new TrackingEvent { Id = Guid.NewGuid().ToString(), TrackingId = t.Id, CycleId = c.Id, Sequence = max + 1, Type = type, RecordedAtUtc = Now, MediaCode = t.MediaCode, FromStateCode = from, ToStateCode = to, OperationId = op };
        if (type is "added" or "new_cycle") e.NewValue = System.Text.Json.JsonSerializer.Serialize(new { c.StartOn, c.CompletionOn });
        db.Events.Add(e); return e;
    }
    private void ValidateDates(DateInput dates)
    {
        if (!dates.ConfirmUnusualDates && (dates.Start > Today || dates.Completion > Today || (dates.Start.HasValue && dates.Completion.HasValue && dates.Completion < dates.Start))) throw new DomainException("unusual_dates");
    }
    internal static void ValidateState(string state) { if (!MediaCodes.States.Contains(state)) throw new DomainException("state_invalid"); }
    internal static void CheckVersion(Domain.Tracking t, long version) { if (t.Version != version) throw new DomainException("conflict"); }
    internal static async Task<Domain.Tracking> FindOwned(AppDbContext db, string owner, string id, CancellationToken ct) => await db.Trackings.SingleOrDefaultAsync(t => t.Id == id && t.IdentityId == owner, ct) ?? throw new DomainException("not_found");
    internal static Task<ConsumptionCycle> CurrentCycle(AppDbContext db, string id, CancellationToken ct) => db.Cycles.Where(c => c.TrackingId == id).OrderByDescending(c => c.Ordinal).FirstAsync(ct);
    private static async Task<TrackingResult> Result(AppDbContext db, Domain.Tracking t, CancellationToken ct) => new(t.Id, t.Version, t.StateCode, (await CurrentCycle(db, t.Id, ct)).Ordinal);
}
