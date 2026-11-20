using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Settings;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Localization;
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
    public string? EditedMedia { get; private set; }
    public long EditedVersion { get; private set; }
    private Dictionary<string, string?> EditedLabels { get; set; } = [];
    private string[] EditedResets { get; set; } = [];

    public string LabelValue(string media, string code, string locale)
    {
        if (media == EditedMedia && EditedLabels.TryGetValue(code, out var value)) return value ?? "";
        return Labels.GetValueOrDefault(media + ":" + code) ?? UiMessages.State(media, code, locale);
    }

    public bool ResetSelected(string media, string code) => media == EditedMedia && EditedResets.Contains(code);
    private async Task Load() { await using var db = new AppDbContext(paths); Labels = await db.StateLabels.ToDictionaryAsync(l => l.MediaCode + ":" + l.StateCode, l => l.Label); }
    public async Task OnGetAsync() { var p = await preferences.GetAsync(); Theme = p.Theme; Locale = p.Locale; Version = p.Version; await Load(); }
    public async Task<IActionResult> OnPostAsync() { try { await preferences.SetAsync(new(OperationId, Version), Theme, Locale, HttpContext.RequestAborted); TempData["Notice"] = "SettingsSaved"; return RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await Load(); return Page(); }
    public async Task<IActionResult> OnPostQuickAsync(string operationId, long version, string theme, string locale) { try { var p = await preferences.SetAsync(new(operationId, version), theme, locale, HttpContext.RequestAborted); return new JsonResult(p); } catch (DomainException e) { return new JsonResult(new { error = e.Code }) { StatusCode = 409 }; } catch (DbUpdateException) { return new JsonResult(new { error = "save_failed" }) { StatusCode = 500 }; } }
    public async Task<IActionResult> OnPostLabelsAsync(string operationId, long version, string media, Dictionary<string, string?>? changes, string[]? resetCodes)
    {
        changes ??= [];
        resetCodes ??= [];
        try
        {
            if (!ModelState.IsValid) throw new DomainException("preference_invalid");
            await labels.SaveMediaAsync(new(operationId, version), media, changes, resetCodes, HttpContext.RequestAborted);
            TempData["Notice"] = "LabelsSaved";
            return RedirectToPage("./Index", null, null, "labels-" + media);
        }
        catch (DomainException e) { ErrorCode = e.Code; }
        catch (DbUpdateException) { ErrorCode = "save_failed"; }

        EditedMedia = media;
        EditedVersion = version;
        EditedLabels = changes;
        EditedResets = resetCodes;
        OperationId = Guid.NewGuid().ToString();
        ModelState.Clear();
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostLabelAsync(string operationId, long version, string media, string code, string? label, bool reset) { try { await labels.RenameAsync(new(operationId, version), media, code, reset ? null : label ?? "", HttpContext.RequestAborted); return RedirectToPage(); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
}
