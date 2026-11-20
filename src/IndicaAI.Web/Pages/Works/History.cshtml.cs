using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
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
    [BindProperty] public bool ConfirmDates { get; set; }
    [BindProperty] public long Version { get; set; }
    [BindProperty] public string OperationId { get; set; } = Guid.NewGuid().ToString(); public string? ErrorCode { get; set; }
    public async Task<IActionResult> OnGetAsync() { if (!await Load()) return NotFound(); Version = Tracking.Version; CycleId = Cycles.Last().Id; return Page(); }
    public async Task<IActionResult> OnPostAsync() { if (!await Load()) return NotFound(); if (!ModelState.IsValid) { ErrorCode = "invalid_date"; return Page(); } try { await service.CorrectDateAsync(new(new(OperationId, Version), Id, CycleId, Field, Value, ConfirmDates), HttpContext.RequestAborted); return RedirectToPage(new { id = Id }); } catch (DomainException e) { ErrorCode = e.Code; return Page(); } catch (DbUpdateException) { ErrorCode = "save_failed"; return Page(); } }
    private async Task<bool> Load() { await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync()).Id; var t = await db.Trackings.SingleOrDefaultAsync(t => t.Id == Id && t.IdentityId == owner); if (t is null) return false; Tracking = t; Work = await db.Works.SingleAsync(w => w.Id == t.WorkId); Cycles = await db.Cycles.Where(c => c.TrackingId == Id).OrderBy(c => c.Ordinal).ToListAsync(); Events = await db.Events.Where(e => e.TrackingId == Id).OrderByDescending(e => e.Sequence).ToListAsync(); return true; }
}

