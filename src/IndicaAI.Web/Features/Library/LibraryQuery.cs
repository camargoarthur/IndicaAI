using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Library;

public sealed class LibraryQuery(DataPaths paths)
{
    public async Task<LibraryResult> ExecuteAsync(LibraryFilter filter, CancellationToken ct = default)
    {
        await using var db = new AppDbContext(paths); var owner = (await db.Identities.SingleAsync(ct)).Id;
        var q = from w in db.Works join t in db.Trackings.Where(t => t.IdentityId == owner) on w.Id equals t.WorkId into tracked from t in tracked.DefaultIfEmpty() select new { Work = w, Tracking = t };
        if (filter.Context != "catalog") q = q.Where(x => x.Tracking != null && (filter.Context == "archived" ? x.Tracking.ArchivedAtUtc != null : x.Tracking.ArchivedAtUtc == null));
        if (filter.Context == "list") q = q.Where(x => x.Tracking != null && db.ListItems.Any(i => i.IdentityId == owner && i.ListId == filter.ListId && i.TrackingId == x.Tracking.Id));
        if (!string.IsNullOrEmpty(filter.Media)) q = q.Where(x => x.Work.MediaCode == filter.Media);
        // Enumerate the bounded local catalogue once; Unicode comparison is independent of SQLite ASCII NOCASE.
        var all = (await q.AsNoTracking().ToListAsync(ct)).Select(x => new LibraryItem(x.Work, x.Tracking)).ToList();
        if (!string.IsNullOrWhiteSpace(filter.Search)) { var key = Text.UnicodeNameKey.Create(filter.Search); all = all.Where(x => Text.UnicodeNameKey.Create(x.Work.Title).Contains(key, StringComparison.Ordinal)).ToList(); }
        var tagLinks = await db.WorkTags.AsNoTracking().ToListAsync(ct); var listLinks = await db.ListItems.Where(i => i.IdentityId == owner).AsNoTracking().ToListAsync(ct);
        bool Matches(LibraryItem x, FilterCriterion c) => c.Kind switch { "tag" => tagLinks.Any(i => i.WorkId == x.Work.Id && i.TagId == c.Value), "list" => x.Tracking != null && listLinks.Any(i => i.TrackingId == x.Tracking.Id && i.ListId == c.Value), "state" => x.Tracking != null && (c.Value.Contains('/') ? x.Work.MediaCode + "/" + x.Tracking.StateCode == c.Value : x.Tracking.StateCode == c.Value), "anime" => x.Work.IsAnime, _ => false };
        all = all.Where(x => (filter.Include.Length == 0 || (filter.Mode == "any" ? filter.Include.Any(c => Matches(x, c)) : filter.Include.All(c => Matches(x, c)))) && !filter.Exclude.Any(c => Matches(x, c))).OrderBy(x => Array.IndexOf(Domain.MediaCodes.All, x.Work.MediaCode)).ThenBy(x => x.Work.Title, StringComparer.CurrentCulture).ToList();
        var size = Math.Clamp(filter.PageSize, 1, 100); var page = Math.Max(1, filter.Page); return new(all.Skip((page - 1) * size).Take(size).ToArray(), all.Count, page, size);
    }
}
