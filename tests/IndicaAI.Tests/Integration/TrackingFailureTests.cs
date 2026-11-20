using System.Data.Common;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IndicaAI.Tests.Integration;

public sealed class TrackingFailureTests
{
    [Fact]
    public async Task CommandFailureRollsBackStateEventAndReceipt()
    {
        var f = new TestDatabase(); await f.InitializeAsync();
        var normal = new TrackingService(f.Paths);
        var t = await normal.AddAsync(new("add", "cidade-distante", "planned", new(null, null, false)));
        var failing = new TrackingService(f.Paths, interceptors: [new FailSecondWrite()]);
        var command = new ChangeState(new("change", t.Version), t.TrackingId, "completed", new(null, null, false));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => failing.ChangeStateAsync(command));
        Assert.Contains("injected", error.InnerException!.Message);
        await using (var db = f.Open())
        {
            Assert.Equal("planned", (await db.Trackings.SingleAsync()).StateCode);
            Assert.Equal(1, await db.Events.CountAsync());
            Assert.False(await db.Receipts.AnyAsync(r => r.OperationId == "change"));
        }
        await normal.ChangeStateAsync(command); await normal.ChangeStateAsync(command);
        await using var fresh = f.Open(); Assert.Equal(2, await fresh.Events.CountAsync());
    }
    private sealed class FailSecondWrite : DbCommandInterceptor
    {
        int count;
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if ((command.CommandText.StartsWith("INSERT") || command.CommandText.StartsWith("UPDATE")) && ++count == 2) throw new InvalidOperationException("injected write failure");
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
