using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Integration;

public sealed class BootstrapTests
{
    [Fact]
    public async Task ReopeningKeepsIdentityAndDoesNotReplaceData()
    {
        var root = Path.Combine(Path.GetTempPath(), "IndicaAI-tests", Guid.NewGuid().ToString("N"));
        var paths = DataPaths.ForRoot(root);
        await new DatabaseBootstrapper(paths).InitializeAsync();
        string id;
        await using (var db = new AppDbContext(paths))
        {
            id = (await db.Identities.SingleAsync()).Id;
            Assert.Equal(5, await db.Media.CountAsync());
            Assert.Equal(25, await db.BaseStates.CountAsync());
            Assert.Equal(5, await db.Works.Select(w => w.MediaCode).Distinct().CountAsync());
            var work = await db.Works.FirstAsync();
            work.Description = "Informação preservada";
            await db.SaveChangesAsync();
        }
        await new DatabaseBootstrapper(paths).InitializeAsync();
        await using var reopened = new AppDbContext(paths);
        Assert.Equal(id, (await reopened.Identities.SingleAsync()).Id);
        Assert.Contains(await reopened.Works.ToListAsync(), w => w.Description == "Informação preservada");
        Assert.Empty(await reopened.Trackings.ToListAsync());
        Assert.Equal("light", (await reopened.Preferences.SingleAsync()).Theme);
        Assert.Equal("pt-BR", (await reopened.Preferences.SingleAsync()).Locale);
    }

    [Fact]
    public async Task DatabaseRejectsAnimeForBooks()
    {
        var paths = DataPaths.ForRoot(Path.Combine(Path.GetTempPath(), "IndicaAI-tests", Guid.NewGuid().ToString("N")));
        await new DatabaseBootstrapper(paths).InitializeAsync();
        await using var db = new AppDbContext(paths);
        var book = await db.Works.FirstAsync(w => w.MediaCode == "livro");
        book.IsAnime = true;
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
