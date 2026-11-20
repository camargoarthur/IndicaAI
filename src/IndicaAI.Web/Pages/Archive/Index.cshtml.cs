using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Archive;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Archive;

public sealed class IndexModel(DataPaths paths, ArchiveService service) : PageModel
{
    public List<(Work Work, Domain.Tracking Tracking)> Items { get; set; } = [];
    public string? ErrorCode { get; set; }
    public async Task OnGetAsync() { await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; Items = (await (from t in db.Trackings join w in db.Works on t.WorkId equals w.Id where t.IdentityId == owner && t.ArchivedAtUtc != null select new { Work = w, Tracking = t }).ToListAsync()).Select(x => (x.Work, x.Tracking)).ToList(); }
    public async Task<IActionResult> OnPostSetAsync(string operationId, string trackingId, long version, bool archived, string? workId) { try { await service.SetAsync(new(operationId, version), trackingId, archived, HttpContext.RequestAborted); return workId is not null ? RedirectToPage("/Works/Details", new { id = workId }) : RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
}
