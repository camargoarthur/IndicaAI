using IndicaAI.Web.Features.Imports;
using IndicaAI.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace IndicaAI.Web.Pages.Admin.Import;

public sealed class ResultModel(ImportApplyService service) : PageModel
{
    public ImportResult Result { get; private set; } = null!;
    public bool RejectedOnly { get; private set; }
    public IEnumerable<ImportRow> Rows => RejectedOnly ? Result.Rows.Where(row => row.Status == "rejected") : Result.Rows;

    public async Task<IActionResult> OnGetAsync(string id, bool rejectedOnly)
    {
        try
        {
            Result = await service.ResultAsync(id, HttpContext.RequestAborted);
            RejectedOnly = rejectedOnly;
            return Page();
        }
        catch (DomainException) { return NotFound(); }
    }
}
