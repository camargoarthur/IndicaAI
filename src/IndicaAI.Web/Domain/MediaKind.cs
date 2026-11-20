namespace IndicaAI.Web.Domain;

public static class MediaCodes
{
    public static readonly string[] All = ["jogo", "livro", "manga", "filme", "serie"];
    public static readonly string[] States = ["planned", "in_progress", "paused", "completed", "dropped"];
}
public sealed class Media { public string Code { get; set; } = ""; }
public sealed class BaseState { public string MediaCode { get; set; } = ""; public string Code { get; set; } = ""; }
public sealed class DomainException(string code, params object[] args) : Exception(code)
{
    public string Code { get; } = code;
    public object[] Arguments { get; } = args;
}
