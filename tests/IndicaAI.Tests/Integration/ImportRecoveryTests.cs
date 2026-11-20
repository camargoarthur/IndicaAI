using System.Text;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Imports;
using IndicaAI.Web.Features.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IndicaAI.Tests.Integration;

public class ImportRecoveryTests
{
    [Fact]
    public async Task CancellationAfterTwoCommitsAndRetentionPreserveRealData()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var group = await new TagService(f.Paths).SaveGroupAsync("g", null, 0, "G"); var previewService = new ImportPreviewService(f.Paths); var csv = "tag_id;nome;grupo_id;midias\n" + string.Join('\n', Enumerable.Range(1, 4).Select(i => $"id{i};Tag {i};{group.Id};jogo")); var p = await previewService.CreateAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)), "tags"); using var cancellation = new CancellationTokenSource(); var r = await new ImportApplyService(f.Paths, [new CancelThirdRow(cancellation)]).ApplyAsync(p.BatchId, p.PlanHash, cancellation.Token); Assert.Equal(2, r.Applied); Assert.Equal(2, r.Unprocessed); Assert.Equal("interrupted", r.Status);
        await new ImportRecoveryService(f.Paths).ReconcileAsync(); await using (var db = f.Open()) { Assert.Equal(2, await db.Tags.CountAsync()); var batch = await db.ImportBatches.SingleAsync(); batch.CompletedAtUtc = DateTime.UtcNow.AddDays(-31).ToString("O"); await db.SaveChangesAsync(); }
        await new ImportRecoveryService(f.Paths).ReconcileAsync(); await using (var db = f.Open()) { Assert.Empty(await db.ImportBatches.ToArrayAsync()); Assert.Equal(2, await db.Tags.CountAsync()); Assert.True(await db.Receipts.AnyAsync(x => x.OperationId == "g")); }
        var again = await previewService.CreateAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)), "tags"); await new ImportApplyService(f.Paths).ApplyAsync(again.BatchId, again.PlanHash); await using (var db = f.Open()) Assert.Equal(4, await db.Tags.CountAsync());
        var cancelled = await previewService.CreateAsync(new MemoryStream(Encoding.UTF8.GetBytes("tag_id;nome\nid1;Alterado\n")), "tags"); await new ImportApplyService(f.Paths).CancelAsync(cancelled.BatchId); await using (var db = f.Open()) Assert.Equal("Tag 1", (await db.Tags.FindAsync("id1"))!.Name);
    }
    sealed class CancelThirdRow(CancellationTokenSource source) : SaveChangesInterceptor
    {
        int saves;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) { if (++saves == 3) { source.Cancel(); throw new OperationCanceledException(source.Token); } return base.SavingChangesAsync(eventData, result, cancellationToken); }
    }
}
