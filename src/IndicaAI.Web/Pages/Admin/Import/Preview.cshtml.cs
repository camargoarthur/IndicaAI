using System.Text.Json;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Imports;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Localization;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Admin.Import;

[RequestSizeLimit(6 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
public sealed class PreviewModel(DataPaths paths, ImportPreviewService preview, ImportApplyService apply) : PageModel
{
    [BindProperty] public IFormFile? Upload { get; set; }
    [BindProperty] public string Type { get; set; } = "tags"; [BindProperty(SupportsGet = true)] public string? Id { get; set; }
    public ImportBatch? Batch { get; set; }
    public List<ImportRow> Rows { get; set; } = []; public List<ImportBatch> Recent { get; set; } = []; public Dictionary<string, string> Groups { get; set; } = []; public string? ErrorCode { get; set; }
    public async Task OnGetAsync() { await using var db = new AppDbContext(paths); Groups = await db.Groups.ToDictionaryAsync(g => g.Id, g => g.Name); Recent = await db.ImportBatches.OrderByDescending(b => b.CreatedAtUtc).Take(10).ToListAsync(); if (Id != null) { Batch = await db.ImportBatches.FindAsync(Id); Rows = await db.ImportRows.Where(r => r.BatchId == Id).OrderBy(r => r.RecordNumber).ToListAsync(); } }
    public async Task<IActionResult> OnPostAsync() { try { if (Upload == null) throw new DomainException("empty_file"); await using var stream = Upload.OpenReadStream(); var p = await preview.CreateAsync(stream, Type, HttpContext.RequestAborted); return RedirectToPage(new { id = p.BatchId }); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
    public async Task<IActionResult> OnPostApplyAsync(string batchId, string planHash) { Id = batchId; try { await apply.ApplyAsync(batchId, planHash, HttpContext.RequestAborted); return RedirectToPage("Result", new { id = batchId }); } catch (DomainException e) { ErrorCode = e.Code; } catch (DbUpdateException) { ErrorCode = "save_failed"; } await OnGetAsync(); return Page(); }
    public async Task<IActionResult> OnPostCancelAsync(string batchId) { await apply.CancelAsync(batchId, HttpContext.RequestAborted); return RedirectToPage(); }
    public string Describe(string? value, UiMessages t)
    {
        if (value == null) return "—";
        using var doc = JsonDocument.Parse(value); var root = doc.RootElement;
        if (root.TryGetProperty("Name", out var name))
        {
            var groupId = root.GetProperty("GroupId").GetString()!;
            var group = Groups.GetValueOrDefault(groupId) ?? groupId;
            return name.GetString() + " · " + group + " · " + string.Join(", ", root.GetProperty("Media").EnumerateArray().Select(m => t["media." + m.GetString()]));
        }
        return root.GetProperty("WorkId").GetString() + " / " + root.GetProperty("TagId").GetString();
    }
    public string Preserved(ImportRow row, UiMessages t) { if (row.Preserved == null) return "—"; if (row.Preserved == "MERGE") return t["ExistingAssociations"]; using var doc = JsonDocument.Parse(row.Preserved); var root = doc.RootElement; var values = new List<string>(); if (root.GetProperty("nome").GetBoolean()) values.Add(t["Name"]); if (root.GetProperty("grupo_id").GetBoolean()) values.Add(t["Group"]); if (root.GetProperty("midias").GetArrayLength() > 0) values.Add(t["ExistingMedia"]); return string.Join(", ", values); }
}

