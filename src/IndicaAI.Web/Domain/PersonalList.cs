namespace IndicaAI.Web.Domain;

public sealed class PersonalList { public string Id { get; set; } = ""; public string IdentityId { get; set; } = ""; public string Name { get; set; } = ""; public long Version { get; set; } = 1; }
public sealed class ListItem { public string IdentityId { get; set; } = ""; public string ListId { get; set; } = ""; public string TrackingId { get; set; } = ""; }
