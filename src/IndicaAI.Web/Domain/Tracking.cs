namespace IndicaAI.Web.Domain;

public sealed class Tracking
{
    public string Id { get; set; } = "";
    public string IdentityId { get; set; } = "";
    public string WorkId { get; set; } = "";
    public string MediaCode { get; set; } = "";
    public string StateCode { get; set; } = "";
    public string IncludedAtUtc { get; set; } = "";
    public DateOnly IncludedOn { get; set; }
    public string Note { get; set; } = "";
    public string? ArchivedAtUtc { get; set; }
    public long Version { get; set; } = 1;
}
public sealed class OperationReceipt
{
    public string IdentityId { get; set; } = "";
    public string OperationId { get; set; } = "";
    public string RequestHash { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Result { get; set; } = "";
    public string RecordedAtUtc { get; set; } = "";
}
