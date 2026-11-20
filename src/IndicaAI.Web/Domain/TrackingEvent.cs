namespace IndicaAI.Web.Domain;

public sealed class TrackingEvent
{
    public string Id { get; set; } = "";
    public string TrackingId { get; set; } = "";
    public string CycleId { get; set; } = "";
    public int Sequence { get; set; }
    public string Type { get; set; } = "";
    public string RecordedAtUtc { get; set; } = "";
    public string MediaCode { get; set; } = "";
    public string? FromStateCode { get; set; }
    public string? ToStateCode { get; set; }
    public string? CorrectedEventId { get; set; }
    public string? Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string OperationId { get; set; } = "";
}
