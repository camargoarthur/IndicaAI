using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Pages.Admin.Associations;

public sealed class IndexModel(DataPaths paths, TagService service) : PageModel
{
    public List<Work> Works { get; private set; } = [];
    public Dictionary<string, List<Tag>> TagsByMedia { get; private set; } = [];
    public List<Association> Links { get; private set; } = [];
    public Work? SelectedWork { get; private set; }
    public string? SelectedTagId { get; private set; }
    public string? ErrorCode { get; private set; }
    public IReadOnlyList<Tag> ApplicableTags => SelectedWork is { } work
        && TagsByMedia.TryGetValue(work.MediaCode, out var tags) ? tags : [];

    public sealed record Association(string WorkId, string WorkTitle, string TagId, string TagName);

    public async Task OnGetAsync(string? workId)
    {
        var ct = HttpContext.RequestAborted;
        await using var db = new AppDbContext(paths);
        Works = await db.Works.OrderBy(w => w.Title).ToListAsync(ct);
        SelectedWork = Works.Find(w => w.Id == workId);
        var applicable = await (from tag in db.Tags
            join media in db.TagMedia on tag.Id equals media.TagId
            orderby tag.Name, tag.Id
            select new { media.MediaCode, Tag = tag }).ToListAsync(ct);
        TagsByMedia = applicable.GroupBy(t => t.MediaCode).ToDictionary(g => g.Key, g => g.Select(t => t.Tag).ToList());
        Links = await (from link in db.WorkTags
            join work in db.Works on link.WorkId equals work.Id
            join tag in db.Tags on link.TagId equals tag.Id
            orderby work.Title, tag.Name
            select new Association(work.Id, work.Title, tag.Id, tag.Name)).ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(string operationId, string? workId, string? tagId, bool add, string? selectedWorkId)
    {
        var workToShow = add ? workId : selectedWorkId;
        try
        {
            if (string.IsNullOrWhiteSpace(workId) || string.IsNullOrWhiteSpace(tagId))
                throw new DomainException("association_selection_required");
            await service.AssociateAsync(operationId, workId, tagId, add, HttpContext.RequestAborted);
            return RedirectToPage(new { workId = workToShow });
        }
        catch (DomainException e) { ErrorCode = e.Code; }
        catch (DbUpdateException) { ErrorCode = "save_failed"; }

        SelectedTagId = add ? tagId : null;
        await OnGetAsync(workToShow);
        return Page();
    }
}
