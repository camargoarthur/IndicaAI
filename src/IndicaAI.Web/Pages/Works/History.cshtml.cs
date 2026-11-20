using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Pages.Library;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Works;

public sealed class HistoryModel(DataPaths paths, TrackingService service) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Id { get; set; } = "";
    public Domain.Tracking Tracking { get; private set; } = null!; public Work Work { get; private set; } = null!;
    public List<ConsumptionCycle> Cycles { get; private set; } = []; public List<TrackingEvent> Events { get; private set; } = [];
    [BindProperty] public string CycleId { get; set; } = ""; [BindProperty] public string Field { get; set; } = "start"; [BindProperty] public DateOnly? Value { get; set; }
    [BindProperty] public string? LoadedDateContext { get; set; }
    [BindProperty] public bool ClearDate { get; set; }
    [BindProperty] public bool ConfirmDates { get; set; }
    [BindProperty] public string DateConfirmationKey { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? BrowseReturnUrl { get; set; }
    public string? SafeBrowseReturnUrl => BrowseOrigin.SafeUrl(Url, BrowseReturnUrl);
    [BindProperty] public long Version { get; set; }
    [BindProperty] public string OperationId { get; set; } = Guid.NewGuid().ToString(); public string? ErrorCode { get; set; }
    public DateWarning DateWarnings { get; private set; }
    public bool OpenCorrection { get; private set; }
    public string SelectedDateContext => $"{CycleId}|{Field}";
    public DateOnly? StoredDate => DateFor(Cycles.SingleOrDefault(c => c.Id == CycleId));
    private DateOnly? CorrectedValue => ClearDate ? null : Value;
    public string ExpectedDateConfirmationKey => FormattableString.Invariant($"{Version}|{SelectedDateContext}|{CorrectedValue:yyyy-MM-dd}");

    public async Task<IActionResult> OnGetAsync(string? cycleId, string? field)
    {
        if (!await Load()) return NotFound();
        Version = Tracking.Version;
        OpenCorrection = cycleId is not null || field is not null;
        CycleId = Cycles.SingleOrDefault(c => c.Id == cycleId)?.Id ?? Cycles.Last().Id;
        Field = field == "completion" ? field : "start";
        ModelState.Remove(nameof(CycleId));
        ModelState.Remove(nameof(Field));
        Value = StoredDate;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await Load()) return NotFound();
        OpenCorrection = true;
        if (Field is not ("start" or "completion") || !Cycles.Any(c => c.Id == CycleId))
        {
            ErrorCode = "invalid_date";
            CycleId = Cycles.Last().Id;
            Field = "start";
            ModelState.Remove(nameof(CycleId));
            ModelState.Remove(nameof(Field));
            ModelState.Remove(nameof(Value));
            Value = StoredDate;
            return Page();
        }
        if (LoadedDateContext is not null && LoadedDateContext != SelectedDateContext)
        {
            ErrorCode = "date_selection_changed";
            ModelState.Remove(nameof(Value));
            Value = StoredDate;
            return Page();
        }
        if (ClearDate) ModelState.Remove(nameof(Value));
        if (!ModelState.IsValid) { ErrorCode = "invalid_date"; return Page(); }
        if (!ClearDate && Value is null) { ErrorCode = "date_value_required"; return Page(); }
        try
        {
            await service.CorrectDateAsync(new(new(OperationId, Version), Id, CycleId, Field, CorrectedValue,
                ConfirmDates && DateConfirmationKey == ExpectedDateConfirmationKey), HttpContext.RequestAborted);
            return RedirectToPage(new { id = Id, cycleId = CycleId, field = Field, browseReturnUrl = SafeBrowseReturnUrl });
        }
        catch (DomainException e) { ErrorCode = e.Code; DateWarnings = DateConfirmation.WarningsFrom(e); }
        catch (DbUpdateException) { ErrorCode = "save_failed"; }
        return Page();
    }
    private DateOnly? DateFor(ConsumptionCycle? cycle) => Field == "completion" ? cycle?.CompletionOn : cycle?.StartOn;

    private async Task<bool> Load() { await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; var t = await db.Trackings.SingleOrDefaultAsync(t => t.Id == Id && t.IdentityId == owner); if (t is null) return false; Tracking = t; Work = await db.Works.SingleAsync(w => w.Id == t.WorkId); Cycles = await db.Cycles.Where(c => c.TrackingId == Id).OrderBy(c => c.Ordinal).ToListAsync(); Events = await db.Events.Where(e => e.TrackingId == Id).OrderByDescending(e => e.Sequence).ToListAsync(); return true; }
}

