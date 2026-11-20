using System.Text.Json;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IndicaAI.Web.Features.Imports;

public sealed class ImportApplyService(DataPaths paths, IInterceptor[]? interceptors = null)
{
    public async Task<ImportResult> ApplyAsync(string batchId, string reviewedPlanHash, CancellationToken ct = default)
    {
        var gate = DatabaseGate.For(paths); await gate.WaitAsync(ct); try
        {
            await using (var db = new AppDbContext(paths))
            {
                var batch = await db.ImportBatches.FindAsync([batchId], ct) ?? throw new DomainException("not_found"); var rows = await db.ImportRows.Where(r => r.BatchId == batchId).OrderBy(r => r.RecordNumber).ToListAsync(ct);
                if (batch.PlanHash != reviewedPlanHash) throw new DomainException("preview_stale"); if (batch.Status != "preview") return await ResultAsync(batchId, CancellationToken.None);
                if (batch.PlanHash != ImportHash.Plan(batch, rows) || batch.BaselineRevision != (await db.Revisions.SingleAsync(ct)).Value) throw new DomainException("preview_stale"); if (rows.All(r => r.Status == "rejected")) throw new DomainException("no_valid_rows"); batch.Status = "applying"; await db.SaveChangesAsync(ct);
            }
            int[] numbers; await using (var db = new AppDbContext(paths)) numbers = await db.ImportRows.Where(r => r.BatchId == batchId && r.Status == "pending").OrderBy(r => r.RecordNumber).Select(r => r.RecordNumber).ToArrayAsync(ct);
            bool interrupted = false;
            foreach (var number in numbers)
            {
                try
                {
                    ct.ThrowIfCancellationRequested(); await using var db = new AppDbContext(paths, interceptors ?? []); await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await db.ImportRows.SingleAsync(r => r.BatchId == batchId && r.RecordNumber == number, ct); var batch = await db.ImportBatches.FindAsync([batchId], ct); bool changed = false;
                    if (row.Action != "unchanged" && row.ReasonCode != "duplicate")
                    {
                        if (batch!.Type == "tags") { var plan = JsonSerializer.Deserialize<TagPlan>(row.After!)!; var result = await TagService.SaveCoreAsync(db, plan.Id, plan.Version, plan.GroupId, plan.Name, plan.Media, true, ct); changed = result.Changed; }
                        else { var plan = JsonSerializer.Deserialize<AssociationPlan>(row.After!)!; changed = await TagService.AssociateCoreAsync(db, plan.WorkId, plan.TagId, true, ct); }
                    }
                    if (changed) await TagService.Bump(db, ct); row.Status = "applied"; await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
                }
                catch (OperationCanceledException) { interrupted = true; break; }
                catch (DomainException e) { await MarkRow(batchId, number, e.Code, e.Arguments); }
                catch (DbUpdateException) { await MarkRow(batchId, number, "save_failed"); }
                catch (Microsoft.Data.Sqlite.SqliteException) { interrupted = true; break; }
            }
            await using (var db = new AppDbContext(paths)) { var batch = await db.ImportBatches.FindAsync([batchId], CancellationToken.None); batch!.Status = interrupted ? "interrupted" : "completed"; batch.CompletedAtUtc = DateTime.UtcNow.ToString("O"); if (interrupted) foreach (var row in await db.ImportRows.Where(r => r.BatchId == batchId && r.Status == "pending").ToListAsync()) row.Status = "unprocessed"; await db.SaveChangesAsync(); }
            return await ResultAsync(batchId, CancellationToken.None);
        }
        finally { gate.Release(); }
    }
    private async Task MarkRow(string id, int number, string reason, object[]? arguments = null)
    {
        await using var db = new AppDbContext(paths);
        var row = await db.ImportRows.SingleAsync(r => r.BatchId == id && r.RecordNumber == number);
        row.Status = "rejected";
        row.ReasonCode = reason;
        row.ReasonArgs = arguments is { Length: > 0 } ? JsonSerializer.Serialize(arguments.Select(a => a.ToString())) : null;
        await db.SaveChangesAsync();
    }
    public async Task<ImportResult> ResultAsync(string batchId, CancellationToken ct = default) { await using var db = new AppDbContext(paths); var batch = await db.ImportBatches.FindAsync([batchId], ct) ?? throw new DomainException("not_found"); var rows = await db.ImportRows.Where(r => r.BatchId == batchId).OrderBy(r => r.RecordNumber).ToListAsync(ct); return new(batchId, batch.Status, rows.Count(r => r.Status == "applied"), rows.Count(r => r.Status == "rejected"), rows.Count(r => r.Status is "pending" or "unprocessed"), batch.IgnoredRecords, rows); }
    public async Task CancelAsync(string batchId, CancellationToken ct = default) { var gate = DatabaseGate.For(paths); await gate.WaitAsync(ct); try { await using var db = new AppDbContext(paths); var batch = await db.ImportBatches.FindAsync([batchId], ct) ?? throw new DomainException("not_found"); if (batch.Status != "preview") throw new DomainException("preview_stale"); batch.Status = "cancelled"; batch.CompletedAtUtc = DateTime.UtcNow.ToString("O"); await db.SaveChangesAsync(ct); } finally { gate.Release(); } }
}

