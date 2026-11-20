using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Library;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Library;

public class BrowseModel(DataPaths paths, LibraryQuery query) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Context { get; set; } = "active";
    [BindProperty(SupportsGet = true)] public string? ListId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Media { get; set; }
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
    public string PageUrl(int page) => Request.Path + QueryString.Create(Request.Query.Where(kv => kv.Key != "PageNumber").SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string?>(kv.Key, v))).Append(new("PageNumber", page.ToString())));
    public async Task OnGetAsync()
    {
        if (Request.Path.StartsWithSegments("/Catalog")) Context = "catalog";
        if(Request.Query.ContainsKey("NewMedia"))Media=Request.Query["NewMedia"].ToString();
        FilterCriterion Parse(string value) { var parts = value.Split(':', 2); return new(parts[0], parts.Length > 1 ? parts[1] : ""); }
        Result = await query.ExecuteAsync(new() { Context = Context, ListId = ListId, Media = Media, Search = Search, Mode = Mode, Include = Include.Select(Parse).ToArray(), Exclude = Exclude.Select(Parse).ToArray(), Page = PageNumber }, HttpContext.RequestAborted);
        await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; Lists = await db.Lists.Where(l => l.IdentityId == owner).ToListAsync(); Tags = await db.Tags.Where(t=>string.IsNullOrEmpty(Media)||db.TagMedia.Any(m=>m.TagId==t.Id&&m.MediaCode==Media)).OrderBy(t => t.Name).ToListAsync();Groups=await db.Groups.ToDictionaryAsync(g=>g.Id,g=>g.Name); Labels = await db.StateLabels.Where(l => l.IdentityId == owner).ToDictionaryAsync(l => l.MediaCode + ":" + l.StateCode, l => l.Label);
    }
}
public sealed class IndexModel(DataPaths paths, LibraryQuery query) : BrowseModel(paths, query);
