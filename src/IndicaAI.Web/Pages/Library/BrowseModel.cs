using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Library;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Library;

public class BrowseModel(DataPaths paths, LibraryQuery query) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Context { get; set; } = "active";
    [BindProperty(SupportsGet = true)] public string? ListId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Media { get; set; }
    [BindProperty(SupportsGet = true)] public string? QuickState { get; set; }
    public bool ShowQuickStates => Context == "active" && MediaCodes.All.Contains(Media);
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string Mode { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public string[] Include { get; set; } = [];
    [BindProperty(SupportsGet = true)] public string[] Exclude { get; set; } = [];
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public LibraryResult Result { get; set; } = new([], 0, 1, 20);
    public List<PersonalList> Lists { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
    public Dictionary<string,string> Groups {get;set;}=[];
    public Dictionary<string, string> Labels { get; set; } = [];
    public string? ListName => Lists.FirstOrDefault(l => l.Id == ListId)?.Name;
    public bool IsAnimeApplicable => string.IsNullOrEmpty(Media) || Media is "filme" or "serie";
    public bool OffersTmdb => Context == "catalog" && (string.IsNullOrEmpty(Media) || Media == "filme");
    public string CurrentUrl => PageNumber == Result.Page ? Request.Path + Request.QueryString : PageUrl(Result.Page);
    public string CatalogUrl => string.IsNullOrEmpty(Media) ? "/Catalog" : QueryHelpers.AddQueryString("/Catalog", "Media", Media);
    public string ClearFiltersUrl => Request.Path + QueryString.Create(new Dictionary<string, string?>
    {
        ["Context"] = Context == "list" ? "list" : null,
        ["ListId"] = Context == "list" ? ListId : null,
        ["Media"] = string.IsNullOrEmpty(Media) ? null : Media,
        ["Search"] = Search,
        ["QuickState"] = QuickState
    }.Where(x => x.Value is not null));
    public string QuickStateUrl(string? state)
    {
        var values = new List<KeyValuePair<string, string?>> { new("Media", Media) };
        if (state is not null) values.Add(new("QuickState", state));
        if (!string.IsNullOrEmpty(Search)) values.Add(new("Search", Search));
        values.Add(new("Mode", Mode));
        values.AddRange(Include.Select(value => new KeyValuePair<string, string?>("Include", value)));
        values.AddRange(Exclude.Select(value => new KeyValuePair<string, string?>("Exclude", value)));
        return Request.Path + QueryString.Create(values);
    }
    public string PageUrl(int page) => Request.Path + QueryString.Create(Request.Query.Where(kv => !kv.Key.Equals("PageNumber", StringComparison.OrdinalIgnoreCase)).SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string?>(kv.Key, v))).Append(new("PageNumber", page.ToString())));
    public async Task OnGetAsync()
    {
        if (Request.Path.StartsWithSegments("/Catalog")) Context = "catalog";
        if (Request.Query.TryGetValue("NewMedia", out var newMedia))
        {
            if (Media != newMedia.ToString()) QuickState = null;
            Media = newMedia.ToString();
        }
        if (!ShowQuickStates || !MediaCodes.States.Contains(QuickState)) QuickState = null;
        if (!IsAnimeApplicable)
        {
            Include = WithoutAnime(Include);
            Exclude = WithoutAnime(Exclude);
        }
        FilterCriterion Parse(string value) { var parts = value.Split(':', 2); return new(parts[0], parts.Length > 1 ? parts[1] : ""); }
        Result = await query.ExecuteAsync(new() { Context = Context, ListId = ListId, Media = Media, QuickState = QuickState, Search = Search, Mode = Mode, Include = Include.Select(Parse).ToArray(), Exclude = Exclude.Select(Parse).ToArray(), Page = PageNumber }, HttpContext.RequestAborted);
        await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; Lists = await db.Lists.Where(l => l.IdentityId == owner).ToListAsync(); Tags = await db.Tags.Where(t=>string.IsNullOrEmpty(Media)||db.TagMedia.Any(m=>m.TagId==t.Id&&m.MediaCode==Media)).OrderBy(t => t.Name).ToListAsync();Groups=await db.Groups.ToDictionaryAsync(g=>g.Id,g=>g.Name); Labels = await db.StateLabels.Where(l => l.IdentityId == owner).ToDictionaryAsync(l => l.MediaCode + ":" + l.StateCode, l => l.Label);
    }

    private static string[] WithoutAnime(string[] values) => values.Where(value => !value.StartsWith("anime:", StringComparison.Ordinal)).ToArray();
}
public sealed class IndexModel(DataPaths paths, LibraryQuery query) : BrowseModel(paths, query);
