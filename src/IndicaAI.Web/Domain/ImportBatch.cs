namespace IndicaAI.Web.Domain;

public sealed class ImportBatch
{
    public string Id { get; set; } = "";
    public string FileHash { get; set; } = "";
    public string PlanHash { get; set; } = "";
    public string ContractVersion { get; set; } = "1";
    public string Type { get; set; } = "";
    public int IgnoredRecords { get; set; }
    public long BaselineRevision { get; set; }
    public string Status { get; set; } = "preview";
    public string CreatedAtUtc { get; set; } = "";
    public string? CompletedAtUtc { get; set; }
}
public sealed class ImportRow
{
    public string BatchId { get; set; } = "";
    public int RecordNumber { get; set; }
    public int PhysicalStartLine { get; set; }
    public string Key { get; set; } = "";
    public string Action { get; set; } = "";
    public string Status { get; set; } = "pending";
    public string? ReasonCode { get; set; }
    public string? ReasonArgs { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public string? Preserved { get; set; }
}
