using IndicaAI.Web.Features.Imports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace IndicaAI.Web.Pages.Admin.Import;

public sealed class ResultModel(ImportApplyService service) : PageModel
{
    public ImportResult Result { get; set; } = null!;
    public async Task<IActionResult> OnGetAsync(string id) { try { Result = await service.ResultAsync(id, HttpContext.RequestAborted); return Page(); } catch (IndicaAI.Web.Domain.DomainException) { return NotFound(); } }
}
