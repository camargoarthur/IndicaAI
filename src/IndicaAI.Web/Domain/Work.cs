namespace IndicaAI.Web.Domain;

public sealed class Work
{
    public string Id { get; set; } = "";
    public string MediaCode { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public bool IsAnime { get; set; }
    public string? CoverAsset { get; set; }
    public int? TmdbMovieId { get; set; }
    public int? TmdbSeriesId { get; set; }
    public long Version { get; set; } = 1;
}
public sealed class WorkCredit { public string WorkId { get; set; } = ""; public string Role { get; set; } = ""; public int Position { get; set; } public string Name { get; set; } = ""; }
public sealed class GameDetail { public string WorkId { get; set; } = ""; public string? Studio { get; set; } }
public sealed class GamePlatform { public string WorkId { get; set; } = ""; public string Name { get; set; } = ""; public string NameKey { get; set; } = ""; }
public sealed class FilmDetail { public string WorkId { get; set; } = ""; public int? DurationMinutes { get; set; } }
