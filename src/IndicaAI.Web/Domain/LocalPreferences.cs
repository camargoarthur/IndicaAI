namespace IndicaAI.Web.Domain;

public sealed class LocalIdentity { public string Id { get; set; } = ""; public string CreatedAtUtc { get; set; } = ""; }
public sealed class UserPreference
{
    public string IdentityId { get; set; } = "";
    public string Theme { get; set; } = "light";
    public string Locale { get; set; } = "pt-BR";
    public long Version { get; set; } = 1;
}
public sealed class StateLabel
{
    public string IdentityId { get; set; } = "";
    public string MediaCode { get; set; } = "";
    public string StateCode { get; set; } = "";
    public string Label { get; set; } = "";
    public string LabelKey { get; set; } = "";
}
