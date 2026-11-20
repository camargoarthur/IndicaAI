using System.Text.Json;
using System.Text.RegularExpressions;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using IndicaAI.Web.Text;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Imports;

public sealed class ImportPreviewService(DataPaths paths)
{
    private static bool ValidId(string id) => Regex.IsMatch(id, @"\A[A-Za-z0-9_.-]{1,128}\z");
    public async Task<ImportPreview> CreateAsync(Stream input, string type, CancellationToken ct = default)
    {
        var document = await CsvInputReader.ReadAsync(input, type, ct); var gate = DatabaseGate.For(paths); await gate.WaitAsync(ct); try
        {
            await using var db = new AppDbContext(paths); var batch = new ImportBatch { Id = Guid.NewGuid().ToString(), FileHash = document.FileHash, Type = type, CreatedAtUtc = DateTime.UtcNow.ToString("O"), BaselineRevision = (await db.Revisions.SingleAsync(ct)).Value };
            batch.IgnoredRecords = document.IgnoredRecords;
            var rows = new List<ImportRow>(); var payloads = new Dictionary<int, string>();
            foreach (var source in document.Records)
            {
                var values = source.Values; string Get(string key) => values.GetValueOrDefault(key) ?? ""; var tagId = Get("tag_id"); var workId = Get("obra_id"); var row = new ImportRow { BatchId = batch.Id, RecordNumber = source.Number, PhysicalStartLine = source.Line, Key = type == "tags" ? tagId : workId + " / " + tagId, Action = "unchanged" }; rows.Add(row);
                if (type == "tags" && values.TryGetValue("midias", out var mediaCell)) values["midias"] = string.Join('|', mediaCell.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().Order());
                payloads[row.RecordNumber] = JsonSerializer.Serialize(values.OrderBy(v => v.Key));
                try
                {
                    if (!ValidId(tagId) || (type == "associacoes" && !ValidId(workId))) throw new DomainException("row_incomplete");
                    if (type == "tags")
                    {
                        var tag = await db.Tags.FindAsync([tagId], ct); var oldMedia = await db.TagMedia.Where(m => m.TagId == tagId).Select(m => m.MediaCode).ToArrayAsync(ct); var name = Get("nome"); var group = Get("grupo_id"); var requested = Get("midias").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToArray();
                        if (requested.Any(m => !MediaCodes.All.Contains(m))) throw new DomainException("media_required");
                        name = name.Length > 0 ? TagService.DisplayName(name) : tag?.Name ?? ""; group = group.Length > 0 ? group : tag?.GroupId ?? ""; var media = oldMedia.Union(requested).Order().ToArray();
                        if (name.Length == 0 || group.Length == 0 || media.Length == 0) throw new DomainException("row_incomplete"); if (!await db.Groups.AnyAsync(g => g.Id == group, ct)) throw new DomainException("reference_missing", group); var key = UnicodeNameKey.Create(name);
                        if (await db.Tags.AnyAsync(t => t.Id != tagId && t.GroupId == group && t.NameKey == key, ct)) throw new DomainException("name_duplicate");
                        var after = new TagPlan(tagId, tag?.Version ?? 0, name, group, media); row.After = JsonSerializer.Serialize(after); row.Before = tag == null ? null : JsonSerializer.Serialize(new TagPlan(tagId, tag.Version, tag.Name, tag.GroupId, oldMedia.Order().ToArray()));
                        row.Action = tag == null ? "created" : row.Before == row.After ? "unchanged" : "updated"; row.Preserved = JsonSerializer.Serialize(new { nome = Get("nome").Length == 0, grupo_id = Get("grupo_id").Length == 0, midias = oldMedia });
                    }
                    else
                    {
                        var work = await db.Works.FindAsync([workId], ct) ?? throw new DomainException("reference_missing", workId); if (!await db.Tags.AnyAsync(t => t.Id == tagId, ct)) throw new DomainException("reference_missing", tagId); if (!await db.TagMedia.AnyAsync(m => m.TagId == tagId && m.MediaCode == work.MediaCode, ct)) throw new DomainException("incompatible");
                        row.After = JsonSerializer.Serialize(new AssociationPlan(workId, tagId)); row.Action = await db.WorkTags.AnyAsync(l => l.WorkId == workId && l.TagId == tagId, ct) ? "unchanged" : "created"; row.Before = row.Action == "unchanged" ? row.After : null; row.Preserved = "MERGE";
                    }
                }
                catch (DomainException e) { Reject(row, e.Code); if (e.Arguments.Length > 0) row.ReasonArgs = JsonSerializer.Serialize(e.Arguments.Select(a => a.ToString()).ToArray()); }
            }
            foreach (var duplicate in rows.GroupBy(r => r.Key).Where(g => g.Count() > 1))
            {
                if (duplicate.Select(r => payloads[r.RecordNumber]).Distinct().Count() > 1) foreach (var row in duplicate) Reject(row, "key_conflict");
                else foreach (var row in duplicate.Skip(1)) { if (row.Status != "rejected") { row.Action = "unchanged"; row.ReasonCode = "duplicate"; } }
            }
            if (type == "tags") foreach (var conflict in rows.Where(r => r.Status != "rejected").GroupBy(r => { var p = JsonSerializer.Deserialize<TagPlan>(r.After!)!; return p.GroupId + "\0" + UnicodeNameKey.Create(p.Name); }).Where(g => g.Select(r => r.Key).Distinct().Count() > 1)) foreach (var row in conflict) Reject(row, "name_duplicate");
            batch.PlanHash = ImportHash.Plan(batch, rows); db.ImportBatches.Add(batch); db.ImportRows.AddRange(rows); await db.SaveChangesAsync(ct); return new(batch.Id, batch.PlanHash, batch.BaselineRevision, rows);
        }
        finally { gate.Release(); }
    }
    private static void Reject(ImportRow row, string reason)
    {
        row.Status = "rejected";
        row.Action = "rejected";
        row.ReasonCode = reason;
        row.ReasonArgs = null;
    }
}

