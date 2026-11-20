using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Imports;

public sealed class ImportRecoveryService(DataPaths paths)
{
    public async Task ReconcileAsync(CancellationToken ct = default)
    {
        var gate = DatabaseGate.For(paths); await gate.WaitAsync(ct); try
        {
            await using var db = new AppDbContext(paths); foreach (var batch in await db.ImportBatches.Where(b => b.Status == "applying").ToListAsync(ct)) { batch.Status = "interrupted"; batch.CompletedAtUtc = DateTime.UtcNow.ToString("O"); foreach (var row in await db.ImportRows.Where(r => r.BatchId == batch.Id && r.Status == "pending").ToListAsync(ct)) row.Status = "unprocessed"; }
            await db.SaveChangesAsync(ct);
            var threshold = DateTime.UtcNow.AddDays(-30).ToString("O"); var expired = await db.ImportBatches.Where(b => b.CompletedAtUtc != null && string.Compare(b.CompletedAtUtc, threshold) < 0).ToListAsync(ct); db.ImportBatches.RemoveRange(expired); await db.SaveChangesAsync(ct);
        }
        finally { gate.Release(); }
    }
}
