using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Lists;

public sealed class IndexModel(DataPaths paths, ListService service) : PageModel
{
    public List<ListSummary> Lists { get; private set; } = [];
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string OperationId { get; set; } = Guid.NewGuid().ToString();
    [BindProperty] public string? Id { get; set; }
    [BindProperty] public long Version { get; set; }
    public string? ErrorCode { get; private set; }

    public sealed record ListSummary(string Id, string Name, long Version, int WorkCount);

    public async Task OnGetAsync()
    {
        var ct = HttpContext.RequestAborted;
        await using var db = new AppDbContext(paths);
        var owner = (await db.Identities.SingleAsync(ct)).Id;
        var activeMemberships = from item in db.ListItems
            join tracking in db.Trackings
                on new { item.IdentityId, item.TrackingId } equals new { tracking.IdentityId, TrackingId = tracking.Id }
            where item.IdentityId == owner && tracking.ArchivedAtUtc == null
            select item;
        Lists = await db.Lists.Where(list => list.IdentityId == owner)
            .OrderBy(list => list.Name)
            .Select(list => new ListSummary(list.Id, list.Name, list.Version,
                activeMemberships.Count(item => item.ListId == list.Id)))
            .ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name)) throw new DomainException("name_required");
            await service.SaveAsync(OperationId, Id, Version, Name, HttpContext.RequestAborted);
            return RedirectToPage();
        }
        catch (DomainException e) { ErrorCode = e.Code; }
        catch (DbUpdateException) { ErrorCode = "save_failed"; }

        await OnGetAsync();
        return Page();
    }
}
