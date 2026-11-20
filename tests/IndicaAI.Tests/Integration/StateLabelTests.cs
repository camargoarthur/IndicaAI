using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Localization;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public sealed class StateLabelTests
{
    [Fact]
    public async Task RenameAndRestoreDoNotChangeStateOrMeaning()
    {
        var f = new TestDatabase(); await f.InitializeAsync();
        var t = await new TrackingService(f.Paths).AddAsync(new("add", "cidade-distante", "in_progress", new(null, null, false)));
        var s = new StateLabelService(f.Paths);
        var version = await s.RenameAsync(new("rename", 1), "livro", "in_progress", "Lendo aos poucos");
        await using (var db = f.Open())
        {
            Assert.Equal("in_progress", (await db.Trackings.SingleAsync()).StateCode);
            Assert.Equal("Lendo aos poucos", (await db.StateLabels.SingleAsync()).Label);
            Assert.Equal(1, await db.Events.CountAsync());
        }
        Assert.Equal("Reading", UiMessages.State("livro", "in_progress", "en"));
        await Assert.ThrowsAsync<DomainException>(() => s.RenameAsync(new("duplicate", version), "livro", "planned", "Lendo aos poucos"));
        await s.RenameAsync(new("restore", version), "livro", "in_progress", null);
        await using var fresh = f.Open(); Assert.Empty(await fresh.StateLabels.ToListAsync()); Assert.Equal(25, await fresh.BaseStates.CountAsync());
    }
}
