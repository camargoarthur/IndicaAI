namespace IndicaAI.Web.Domain;

public sealed class TagGroup { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string NameKey { get; set; } = ""; public long Version { get; set; } = 1; }
public sealed class Tag { public string Id { get; set; } = ""; public string GroupId { get; set; } = ""; public string Name { get; set; } = ""; public string NameKey { get; set; } = ""; public long Version { get; set; } = 1; }
public sealed class TagMedia { public string TagId { get; set; } = ""; public string MediaCode { get; set; } = ""; }
public sealed class WorkTag { public string WorkId { get; set; } = ""; public string TagId { get; set; } = ""; public string MediaCode { get; set; } = ""; }
public sealed class ClassificationRevision { public int Id { get; set; } = 1; public long Value { get; set; } }
