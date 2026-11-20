using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Admin.Associations;

public sealed class IndexModel(DataPaths paths, TagService service) : PageModel
{
    public List<Work> Works { get; set; } = []; public List<Tag> Tags { get; set; } = []; public List<WorkTag> Links { get; set; } = []; public string? ErrorCode { get; set; }
    public async Task OnGetAsync() { await using var db = new AppDbContext(paths); Works = await db.Works.OrderBy(w => w.Title).ToListAsync(); Tags = await db.Tags.OrderBy(t => t.Name).ToListAsync(); Links = await db.WorkTags.ToListAsync(); }
    public async Task<IActionResult> OnPostAsync(string operationId, string workId, string tagId, bool add) { try { await service.AssociateAsync(operationId, workId, tagId, add, HttpContext.RequestAborted); return RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
}
