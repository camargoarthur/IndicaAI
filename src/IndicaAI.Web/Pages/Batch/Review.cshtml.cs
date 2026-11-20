using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Batch;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Batch;

public sealed class ReviewModel(DataPaths paths, BatchService service) : PageModel
{
    [BindProperty(SupportsGet = true)] public string[] Ids { get; set; } = [];
    [BindProperty] public List<BatchItem> Items { get; set; } = [];
    [BindProperty] public Dictionary<string, string> Targets { get; set; } = [];
    [BindProperty] public string OperationId { get; set; } = Guid.NewGuid().ToString();
    [BindProperty] public string ActionKind { get; set; } = "states";
    [BindProperty] public string ListId { get; set; } = "";
    [BindProperty] public bool Add { get; set; } = true;
    [BindProperty] public bool KeepCycle { get; set; }
    public List<Domain.Tracking> Trackings { get; set; } = [];
    public List<Work> Works { get; set; } = [];
    public List<PersonalList> Lists { get; set; } = [];
    public string? ErrorCode { get; set; }
    public async Task OnGetAsync() { await Load(Ids); Items = Trackings.Select(t => new BatchItem(t.Id, t.Version)).ToList(); }
    public async Task<IActionResult> OnPostRemoveAsync(string media)
    {
        await Load(Items.Select(i => i.TrackingId).ToArray());
        var remove = Trackings.Where(t => t.MediaCode == media).Select(t => t.Id).ToArray();
        Items = Items.Where(i => !remove.Contains(i.TrackingId)).ToList(); Targets.Remove(media); OperationId = Guid.NewGuid().ToString();
        ModelState.Clear(); await Load(Items.Select(i => i.TrackingId).ToArray()); return Page();
    }
    private async Task Load(string[] ids) { await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; Trackings = await db.Trackings.Where(t => ids.Contains(t.Id) && t.IdentityId == owner && t.ArchivedAtUtc == null).ToListAsync(); var workIds = Trackings.Select(t => t.WorkId).ToArray(); Works = await db.Works.Where(w => workIds.Contains(w.Id)).ToListAsync(); Lists = await db.Lists.Where(l => l.IdentityId == owner).ToListAsync(); }
    public async Task<IActionResult> OnPostAsync() { try { if (ActionKind == "lists") await service.ChangeListsAsync(OperationId, Items, ListId, Add, HttpContext.RequestAborted); else await service.ChangeStatesAsync(OperationId, Items, Targets, KeepCycle, HttpContext.RequestAborted); TempData["Notice"] = "Saved"; return RedirectToPage("/Library/Index"); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await Load(Items.Select(i => i.TrackingId).ToArray()); return Page(); }
}
