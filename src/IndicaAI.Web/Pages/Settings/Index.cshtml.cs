using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Settings;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Settings;

public sealed class IndexModel(DataPaths paths, PreferenceService preferences, StateLabelService labels) : PageModel
{
    [BindProperty] public string Theme { get; set; } = "light"; [BindProperty] public string Locale { get; set; } = "pt-BR";
    [BindProperty] public string OperationId { get; set; } = Guid.NewGuid().ToString(); [BindProperty] public long Version { get; set; }
    public Dictionary<string, string> Labels { get; set; } = []; public string? ErrorCode { get; set; }
    private async Task Load() { await using var db = new AppDbContext(paths); Labels = await db.StateLabels.ToDictionaryAsync(l => l.MediaCode + ":" + l.StateCode, l => l.Label); }
    public async Task OnGetAsync() { var p = await preferences.GetAsync(); Theme = p.Theme; Locale = p.Locale; Version = p.Version; await Load(); }
    public async Task<IActionResult> OnPostAsync() { try { await preferences.SetAsync(new(OperationId, Version), Theme, Locale, HttpContext.RequestAborted); TempData["Notice"] = "SettingsSaved"; return RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await Load(); return Page(); }
    public async Task<IActionResult> OnPostQuickAsync(string operationId, long version, string theme, string locale) { try { var p = await preferences.SetAsync(new(operationId, version), theme, locale, HttpContext.RequestAborted); return new JsonResult(p); } catch (DomainException e) { return new JsonResult(new { error = e.Code }) { StatusCode = 409 }; } catch (DbUpdateException) { return new JsonResult(new { error = "save_failed" }) { StatusCode = 500 }; } }
    public async Task<IActionResult> OnPostLabelAsync(string operationId, long version, string media, string code, string? label, bool reset) { try { await labels.RenameAsync(new(operationId, version), media, code, reset ? null : label ?? "", HttpContext.RequestAborted); return RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
}
