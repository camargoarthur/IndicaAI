namespace IndicaAI.Web.Domain;

public sealed class ConsumptionCycle
{
    public string Id { get; set; } = "";
    public string TrackingId { get; set; } = "";
    public int Ordinal { get; set; }
    public string CreatedAtUtc { get; set; } = "";
    public DateOnly? StartOn { get; set; }
    public DateOnly? CompletionOn { get; set; }
    public long Version { get; set; } = 1;
}
