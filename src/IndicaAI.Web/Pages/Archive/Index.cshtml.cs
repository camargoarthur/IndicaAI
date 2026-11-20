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
    public List<(Work Work, Domain.Tracking Tracking)> Items { get; private set; } = [];
    public Work? RestoredWork { get; private set; }
    public string? ErrorCode { get; private set; }

    public Task OnGetAsync() => LoadAsync(TempData["RestoredTrackingId"]?.ToString());

    private async Task LoadAsync(string? restoredTrackingId = null)
    {
        var ct = HttpContext.RequestAborted;
        await using var db = new AppDbContext(paths);
        var owner = (await db.Identities.SingleAsync(ct)).Id;
        var owned = from tracking in db.Trackings
            join work in db.Works on tracking.WorkId equals work.Id
            where tracking.IdentityId == owner
            select new { Work = work, Tracking = tracking };
        Items = (await owned.Where(item => item.Tracking.ArchivedAtUtc != null).ToListAsync(ct))
            .Select(item => (item.Work, item.Tracking)).ToList();
        if (restoredTrackingId is not null)
            RestoredWork = await owned.Where(item => item.Tracking.Id == restoredTrackingId && item.Tracking.ArchivedAtUtc == null)
                .Select(item => item.Work).SingleOrDefaultAsync(ct);
    }

    public async Task<IActionResult> OnPostSetAsync(string operationId, string trackingId, long version, bool archived, string? workId)
    {
        try
        {
            var result = await service.SetAsync(new(operationId, version), trackingId, archived, HttpContext.RequestAborted);
            if (workId is not null) return RedirectToPage("/Works/Details", new { id = workId });
            if (!archived) TempData["RestoredTrackingId"] = result.TrackingId;
            return RedirectToPage();
        }
        catch (DomainException e) { ErrorCode = e.Code; }
        catch (DbUpdateException) { ErrorCode = "save_failed"; }

        await LoadAsync();
        return Page();
    }
}
