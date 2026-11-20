using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Pages.Library;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Works;

public sealed class NewCycleModel(DataPaths paths, TrackingService service) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Id { get; set; } = ""; [BindProperty] public DetailsModel.TrackingInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? BrowseReturnUrl { get; set; }
    public string? OriginUrl => BrowseOrigin.SafeUrl(Url, BrowseReturnUrl);
    public Domain.Tracking Tracking { get; private set; } = null!; public Work Work { get; private set; } = null!; public string? ErrorCode { get; set; }
    public DateWarning DateWarnings { get; private set; }
    private async Task<bool> Load() { await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; var t = await db.Trackings.SingleOrDefaultAsync(t => t.Id == Id && t.IdentityId == owner); if (t is null) return false; Tracking = t; Work = await db.Works.SingleAsync(w => w.Id == t.WorkId); return true; }
    public async Task<IActionResult> OnGetAsync() { if (!await Load()) return NotFound(); Input.Version = Tracking.Version; return Page(); }
    public async Task<IActionResult> OnPostDatesAsync()
    {
        if (!await Load()) return NotFound();
        PrepareDateFields();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await Load()) return NotFound();
        PrepareDateFields();
        if (!ModelState.IsValid) { ErrorCode = "invalid_date"; return Page(); }
        try
        {
            await service.StartCycleAsync(new(new(Input.OperationId, Input.Version), Id, Input.StateCode, Input.Dates()), HttpContext.RequestAborted);
            return RedirectToPage("Details", new { id = Work.Id, BrowseReturnUrl = OriginUrl });
        }
        catch (DomainException e) { ErrorCode = e.Code; DateWarnings = DateConfirmation.WarningsFrom(e); }
        catch (DbUpdateException) { ErrorCode = "save_failed"; }
        return Page();
    }

    private void PrepareDateFields()
    {
        if (!TrackingDateForm.Posted(Request.Form, ModelState, nameof(Input.Start), Input.ShowStart)) Input.Start = null;
        if (!TrackingDateForm.Posted(Request.Form, ModelState, nameof(Input.Completion), Input.ShowCompletion)) Input.Completion = null;
    }
}

