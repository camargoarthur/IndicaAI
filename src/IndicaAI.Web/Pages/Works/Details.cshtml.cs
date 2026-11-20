using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Pages.Library;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Works;

public sealed class DetailsModel(DataPaths paths, TrackingService service) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Id { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? BrowseReturnUrl { get; set; }
    public string? OriginUrl => BrowseOrigin.SafeUrl(Url, BrowseReturnUrl);
    [BindProperty] public TrackingInput Input { get; set; } = new();
    public Work Work { get; private set; } = null!;
    public Domain.Tracking? Tracking { get; private set; }
    public ConsumptionCycle? Cycle { get; private set; }
    public List<PersonalList> Lists { get; private set; } = [];
    public Dictionary<string, string> Labels { get; private set; } = [];
    public List<WorkCredit> Credits { get; private set; } = [];
    public GameDetail? Game { get; private set; }
    public FilmDetail? Film { get; private set; }
    public string[] Platforms { get; private set; } = [];
    public string[] TagNames { get; private set; } = [];
    public string? ErrorCode { get; set; }
    public DateWarning DateWarnings { get; private set; }
    public async Task<IActionResult> OnGetAsync() => await Load(true) ? Page() : NotFound();
    public async Task<IActionResult> OnPostDatesAsync()
    {
        if (!await Load(false)) return NotFound();
        PrepareDateFields();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!await Load(false)) return NotFound();
        var (startPosted, completionPosted) = PrepareDateFields();
        if (!ModelState.IsValid) { ErrorCode = "invalid_date"; return Page(); }
        try
        {
            var dates = Input.Dates();
            // Version zero identifies the original inclusion intent, including a lost-response retry.
            if (Input.Version == 0) await service.AddAsync(new(Input.OperationId, Id, Input.StateCode, dates, Input.Note, Input.ListIds), HttpContext.RequestAborted);
            else if (Tracking is not null) await service.ChangeStateAsync(new(new(Input.OperationId, Input.Version), Tracking.Id, Input.StateCode, dates,
                Input.Note, Input.ListIds, Input.KeepCycle, PreserveStart: !startPosted, PreserveCompletion: !completionPosted), HttpContext.RequestAborted);
            else throw new DomainException("not_found");
            TempData["Notice"] = "Saved"; return RedirectToPage(new { id = Id, BrowseReturnUrl = OriginUrl });
        }
        catch (DomainException e) { ErrorCode = e.Code; DateWarnings = DateConfirmation.WarningsFrom(e); }
        catch (DbUpdateException) { ErrorCode = "save_failed"; }
        return Page();
    }

    private (bool Start, bool Completion) PrepareDateFields()
    {
        var startPosted = TrackingDateForm.Posted(Request.Form, ModelState, nameof(Input.Start), Input.ShowStart);
        var completionPosted = TrackingDateForm.Posted(Request.Form, ModelState, nameof(Input.Completion), Input.ShowCompletion);
        if (!startPosted) Input.Start = Input.Version == 0 ? null : Cycle?.StartOn;
        if (!completionPosted) Input.Completion = Input.Version == 0 ? null : Cycle?.CompletionOn;
        return (startPosted, completionPosted);
    }

    private async Task<bool> Load(bool populate)
    {
        await using var db = new AppDbContext(paths);
        var w = await db.Works.FindAsync([Id], HttpContext.RequestAborted); if (w is null) return false; Work = w;
        Credits = await db.Credits.Where(c => c.WorkId == Id).OrderBy(c => c.Position).ToListAsync(); Game = await db.Games.FindAsync(Id); Film = await db.Films.FindAsync(Id); Platforms = await db.Platforms.Where(p => p.WorkId == Id).Select(p => p.Name).ToArrayAsync(); TagNames = await (from link in db.WorkTags join tag in db.Tags on link.TagId equals tag.Id where link.WorkId == Id select tag.Name).ToArrayAsync();
        var owner = (await db.Identities.SingleAsync()).Id;
        Tracking = await db.Trackings.SingleOrDefaultAsync(t => t.WorkId == Id && t.IdentityId == owner);
        if (Tracking is not null) Cycle = await db.Cycles.Where(c => c.TrackingId == Tracking.Id).OrderByDescending(c => c.Ordinal).FirstAsync();
        Lists = await db.Lists.Where(l => l.IdentityId == owner).OrderBy(l => l.Name).ToListAsync();
        Labels = await db.StateLabels.Where(l => l.IdentityId == owner && l.MediaCode == Work.MediaCode).ToDictionaryAsync(l => l.StateCode, l => l.Label);
        if (populate && Tracking is not null) Input = new() { Version = Tracking.Version, StateCode = Tracking.StateCode, Start = Cycle?.StartOn, Completion = Cycle?.CompletionOn, Note = Tracking.Note, ListIds = await db.ListItems.Where(i => i.TrackingId == Tracking.Id).Select(i => i.ListId).ToArrayAsync() };
        return true;
    }
    public sealed class TrackingInput
    {
        public string OperationId { get; set; } = Guid.NewGuid().ToString(); public long Version { get; set; }
        public string StateCode { get; set; } = "";
        public DateOnly? Start { get; set; }
        public DateOnly? Completion { get; set; }
        public bool ConfirmDates { get; set; }
        public string DateConfirmationKey { get; set; } = "";
        public bool KeepCycle { get; set; }
        public string Note { get; set; } = ""; public string[] ListIds { get; set; } = [];

        public bool ShowStart => TrackingDateForm.ShowsStart(StateCode);
        public bool ShowCompletion => TrackingDateForm.ShowsCompletion(StateCode);
        public string ExpectedDateConfirmationKey => FormattableString.Invariant($"{Version}|{StateCode}|{(ShowStart ? Start : null):yyyy-MM-dd}|{(ShowCompletion ? Completion : null):yyyy-MM-dd}");

        public DateInput Dates() => new(ShowStart ? Start : null, ShowCompletion ? Completion : null,
            ConfirmDates && DateConfirmationKey == ExpectedDateConfirmationKey);
    }
}
