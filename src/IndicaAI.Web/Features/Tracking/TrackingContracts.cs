namespace IndicaAI.Web.Features.Tracking;

public sealed record Mutation(string OperationId, long ExpectedVersion);
public sealed record TrackingResult(string TrackingId, long Version, string StateCode, int CycleOrdinal);
public sealed record DateInput(DateOnly? Start, DateOnly? Completion, bool ConfirmUnusualDates);
public sealed record AddTracking(string OperationId, string WorkId, string StateCode, DateInput Dates, string Note = "", string[]? ListIds = null);
public sealed record ChangeState(Mutation Mutation, string TrackingId, string StateCode, DateInput Dates, string? Note = null, string[]? ListIds = null, bool ConfirmKeepCycle = false);
public sealed record StartCycle(Mutation Mutation, string TrackingId, string StateCode, DateInput Dates);
public sealed record CorrectDate(Mutation Mutation, string TrackingId, string CycleId, string Field, DateOnly? Value, bool ConfirmUnusualDates);
public sealed record SaveNote(Mutation Mutation, string TrackingId, string Text);
