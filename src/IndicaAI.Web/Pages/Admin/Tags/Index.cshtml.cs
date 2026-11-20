using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Admin.Tags;

public sealed class IndexModel(DataPaths paths, TagService service) : PageModel
{
    public List<TagGroup> Groups { get; set; } = []; public List<Tag> Tags { get; set; } = []; public List<TagMedia> Media { get; set; } = [];
    [BindProperty] public string OperationId { get; set; } = Guid.NewGuid().ToString(); [BindProperty] public string? Id { get; set; }
    [BindProperty] public long Version { get; set; }
    [BindProperty] public string Name { get; set; } = ""; [BindProperty] public string GroupId { get; set; } = ""; [BindProperty] public string[] Applicable { get; set; } = [];
    public string? ErrorCode { get; set; }
    public object[] ErrorArgs { get; set; } = [];
    public async Task OnGetAsync() { await using var db = new AppDbContext(paths); Groups = await db.Groups.OrderBy(g => g.Name).ToListAsync(); Tags = await db.Tags.OrderBy(t => t.Name).ToListAsync(); Media = await db.TagMedia.ToListAsync(); }
    public async Task<IActionResult> OnPostAsync(string kind) { try { if (kind == "group") await service.SaveGroupAsync(OperationId, Id, Version, Name, HttpContext.RequestAborted); else await service.SaveAsync(new(OperationId, Id, Version, GroupId, Name, Applicable), HttpContext.RequestAborted); return RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; ErrorArgs = e.Arguments; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
}

