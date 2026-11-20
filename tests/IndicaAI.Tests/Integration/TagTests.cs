using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tags;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public class TagTests
{
    [Fact]
    public async Task UnicodeUniquenessAndApplicabilityAreEnforcedWithoutPartialChanges()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var service = new TagService(f.Paths);
        var group = await service.SaveGroupAsync("group", null, 0, "Gênero");
        var tag = await service.SaveAsync(new("tag", null, 0, group.Id, " AÇÃO ", ["jogo", "livro"]));
        await Assert.ThrowsAsync<DomainException>(() => service.SaveAsync(new("duplicate", null, 0, group.Id, "ac\u0327a\u0303o", ["jogo"])));
        await service.AssociateAsync("assoc", "alem-do-vale", tag.Id, true); await service.AssociateAsync("assoc", "alem-do-vale", tag.Id, true);
        var error = await Assert.ThrowsAsync<DomainException>(() => service.SaveAsync(new("reduce", tag.Id, tag.Version, group.Id, "ação", ["livro"]))); Assert.Equal("linked_media", error.Code);
        await Assert.ThrowsAsync<DomainException>(() => service.AssociateAsync("bad", "rota-do-norte", tag.Id, true));
        await using var db = f.Open(); Assert.Equal(1, await db.Tags.CountAsync()); Assert.Equal(2, await db.TagMedia.CountAsync()); Assert.Equal(1, await db.WorkTags.CountAsync()); Assert.Equal(3, (await db.Revisions.SingleAsync()).Value);
    }
}
