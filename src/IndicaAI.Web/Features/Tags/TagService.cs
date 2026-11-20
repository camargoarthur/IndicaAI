using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using IndicaAI.Web.Text;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Tags;

public record TagCommand(string OperationId, string? Id, long ExpectedVersion, string GroupId, string Name, string[] Media);
public record GlobalResult(string Id, long Version);
public sealed class TagService(DataPaths paths)
{
    private readonly CommandRunner runner = new(paths);
    public Task<GlobalResult> SaveGroupAsync(string op, string? id, long version, string name, CancellationToken ct = default) => runner.RunAsync(op, "group.save", new { id, version, name }, async (db, _) =>
    {
        name = DisplayName(name); var key = UnicodeNameKey.Create(name); if (await db.Groups.AnyAsync(g => g.Id != id && g.NameKey == key, ct)) throw new DomainException("name_duplicate");
        TagGroup group; if (id == null) { group = new() { Id = Guid.NewGuid().ToString(), Name = name, NameKey = key }; db.Groups.Add(group); } else { group = await db.Groups.FindAsync([id], ct) ?? throw new DomainException("not_found"); if (group.Version != version) throw new DomainException("conflict"); if (group.Name == name) return new GlobalResult(group.Id, group.Version); group.Name = name; group.NameKey = key; group.Version++; }
        await Bump(db, ct); return new GlobalResult(group.Id, group.Version);
    }, ct);
    public Task<GlobalResult> SaveAsync(TagCommand command, CancellationToken ct = default) => runner.RunAsync(command.OperationId, "tag.save", command, async (db, _) =>
    {
        var result = await SaveCoreAsync(db, command.Id, command.ExpectedVersion, command.GroupId, command.Name, command.Media, false, ct); if (result.Changed) await Bump(db, ct); return new GlobalResult(result.Tag.Id, result.Tag.Version);
    }, ct);
    internal static async Task<(Tag Tag, bool Changed)> SaveCoreAsync(AppDbContext db, string? id, long version, string groupId, string name, string[] media, bool createWithId, CancellationToken ct)
    {
        name = DisplayName(name); var key = UnicodeNameKey.Create(name); media = media.Distinct().Order().ToArray(); if (media.Length == 0 || media.Any(m => !MediaCodes.All.Contains(m))) throw new DomainException("media_required");
        if (!await db.Groups.AnyAsync(g => g.Id == groupId, ct)) throw new DomainException("reference_missing");
        if (await db.Tags.AnyAsync(t => t.Id != id && t.GroupId == groupId && t.NameKey == key, ct)) throw new DomainException("name_duplicate");
        var tag = id is null ? null : await db.Tags.FindAsync([id], ct); if (tag is null && id is not null && !createWithId) throw new DomainException("not_found");
        if (tag is null) { tag = new() { Id = id ?? Guid.NewGuid().ToString(), GroupId = groupId, Name = name, NameKey = key }; db.Tags.Add(tag); foreach (var m in media) db.TagMedia.Add(new() { TagId = tag.Id, MediaCode = m }); return (tag, true); }
        if (tag.Version != version) throw new DomainException("conflict"); var oldMedia = await db.TagMedia.Where(m => m.TagId == id).ToListAsync(ct); var removed = oldMedia.Where(m => !media.Contains(m.MediaCode)).ToArray(); var removedCodes = removed.Select(m => m.MediaCode).ToArray();
        var affected = await (from link in db.WorkTags join w in db.Works on link.WorkId equals w.Id where link.TagId == id && removedCodes.Contains(link.MediaCode) select w.Title).ToArrayAsync(ct); if (affected.Length > 0) throw new DomainException("linked_media", string.Join(", ", affected));
        bool changed = tag.Name != name || tag.GroupId != groupId || !oldMedia.Select(m => m.MediaCode).Order().SequenceEqual(media);
        if (changed) { tag.Name = name; tag.NameKey = key; tag.GroupId = groupId; tag.Version++; db.TagMedia.RemoveRange(removed); foreach (var m in media.Where(m => !oldMedia.Any(o => o.MediaCode == m))) db.TagMedia.Add(new() { TagId = tag.Id, MediaCode = m }); }
        return (tag, changed);
    }
    public Task<bool> AssociateAsync(string operationId, string workId, string tagId, bool add = true, CancellationToken ct = default) => runner.RunAsync(operationId, "tag.associate", new { workId, tagId, add }, async (db, _) => { var changed = await AssociateCoreAsync(db, workId, tagId, add, ct); if (changed) await Bump(db, ct); return changed; }, ct);
    internal static async Task<bool> AssociateCoreAsync(AppDbContext db, string workId, string tagId, bool add, CancellationToken ct) { var work = await db.Works.FindAsync([workId], ct) ?? throw new DomainException("reference_missing"); if (!await db.Tags.AnyAsync(t => t.Id == tagId, ct)) throw new DomainException("reference_missing"); var link = await db.WorkTags.FindAsync([workId, tagId], ct); if (add) { if (!await db.TagMedia.AnyAsync(m => m.TagId == tagId && m.MediaCode == work.MediaCode, ct)) throw new DomainException("incompatible"); if (link != null) return false; db.WorkTags.Add(new() { WorkId = workId, TagId = tagId, MediaCode = work.MediaCode }); } else { if (link == null) return false; db.WorkTags.Remove(link); } return true; }
    internal static string DisplayName(string name) { name = name.Trim().Normalize(); if (name.Length == 0) throw new DomainException("name_required"); if (name.Length > 200) throw new DomainException("name_required"); return name; }
    internal static async Task Bump(AppDbContext db, CancellationToken ct) => (await db.Revisions.SingleAsync(ct)).Value++;
}

