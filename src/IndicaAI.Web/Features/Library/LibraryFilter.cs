using IndicaAI.Web.Domain;
namespace IndicaAI.Web.Features.Library;

public record FilterCriterion(string Kind, string Value);
public record LibraryFilter
{
    public string Context { get; init; } = "active";
    public string? ListId { get; init; }
    public string? Media { get; init; }
    public string? QuickState { get; init; }
    public string? Search { get; init; }
    public string Mode { get; init; } = "all";
    public FilterCriterion[] Include { get; init; } = [];
    public FilterCriterion[] Exclude { get; init; } = [];
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
public record LibraryItem(Work Work, Domain.Tracking? Tracking);
public record LibraryResult(IReadOnlyList<LibraryItem> Items, int Total, int Page, int PageSize);
