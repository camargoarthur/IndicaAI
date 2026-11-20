using System.Data.Common;
using System.Text;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Imports;
using IndicaAI.Web.Features.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IndicaAI.Tests.Integration;

public class ImportFailureTests
{
    [Fact]
    public async Task RowFailureDoesNotUndoOtherCommitsAndInterruptedReportIsRecovered()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var group = await new TagService(f.Paths).SaveGroupAsync("g", null, 0, "G");
        var csv = $"tag_id;nome;grupo_id;midias\none;One;{group.Id};jogo\nfail;Fail;{group.Id};jogo\ntwo;Two;{group.Id};livro\n";
        var preview = await new ImportPreviewService(f.Paths).CreateAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)), "tags");
        var r = await new ImportApplyService(f.Paths, [new FailTag()]).ApplyAsync(preview.BatchId, preview.PlanHash); Assert.Equal(2, r.Applied); Assert.Equal(1, r.Rejected);
        await using (var db = f.Open())
        {
            Assert.Equal(new[] { "one", "two" }, await db.Tags.OrderBy(t => t.Id).Select(t => t.Id).ToArrayAsync()); Assert.False(await db.TagMedia.AnyAsync(m => m.TagId == "fail"));
            var batch = await db.ImportBatches.FindAsync(preview.BatchId); batch!.Status = "applying"; batch.CompletedAtUtc = null; var row = await db.ImportRows.SingleAsync(x => x.BatchId == batch.Id && x.Key == "fail"); row.Status = "pending"; await db.SaveChangesAsync();
        }
        await new ImportRecoveryService(f.Paths).ReconcileAsync(); var report = await new ImportApplyService(f.Paths).ResultAsync(preview.BatchId); Assert.Equal("interrupted", report.Status); Assert.Equal(2, report.Applied); Assert.Equal(1, report.Unprocessed);
    }
    private sealed class FailTag : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) { if (command.CommandText.StartsWith("INSERT INTO \"TagMedia\"") && command.Parameters.Cast<DbParameter>().Any(p => p.Value?.ToString() == "fail")) throw new InvalidOperationException("injected row failure"); return base.ReaderExecutingAsync(command, eventData, result, cancellationToken); }
    }
}

