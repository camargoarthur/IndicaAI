using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Lists;

public sealed class IndexModel(DataPaths paths, ListService service) : PageModel
{
    public List<PersonalList> Lists { get; set; } = [];
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string OperationId { get; set; } = Guid.NewGuid().ToString();
    [BindProperty] public string? Id { get; set; }
    [BindProperty] public long Version { get; set; }
    public string? ErrorCode { get; set; }
    public async Task OnGetAsync() { await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; Lists = await db.Lists.Where(l => l.IdentityId == owner).OrderBy(l => l.Name).ToListAsync(); }
    public async Task<IActionResult> OnPostAsync() { try { await service.SaveAsync(OperationId, Id, Version, Name, HttpContext.RequestAborted); return RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
}
