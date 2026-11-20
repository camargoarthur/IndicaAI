using System.Text;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Imports;
using IndicaAI.Web.Features.Tags;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public class ImportTests
{
    static MemoryStream Csv(string text) => new(Encoding.UTF8.GetBytes(text));
    [Fact]
    public async Task EquivalentMediaPayloadsAreIdenticalDuplicates()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var group = await new TagService(f.Paths).SaveGroupAsync("g", null, 0, "G");
        var plan = await new ImportPreviewService(f.Paths).CreateAsync(Csv($"tag_id;nome;grupo_id;midias\nx;Ação;{group.Id};jogo|livro\nx;Ação;{group.Id};livro|jogo|jogo\n"), "tags");
        Assert.DoesNotContain(plan.Rows, r => r.Status == "rejected"); await new ImportApplyService(f.Paths).ApplyAsync(plan.BatchId, plan.PlanHash); await using var db = f.Open(); Assert.Single(await db.Tags.ToArrayAsync()); Assert.Equal(2, await db.TagMedia.CountAsync());
    }
    [Fact]
    public async Task AbsentFieldsArePreservedAndPreviewIsReadOnly()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tags = new TagService(f.Paths); var group = await tags.SaveGroupAsync("g", null, 0, "Gênero"); var tag = await tags.SaveAsync(new("t", null, 0, group.Id, "Inicial", ["jogo", "livro"]));
        var preview = new ImportPreviewService(f.Paths); var apply = new ImportApplyService(f.Paths);
        var plan = await preview.CreateAsync(Csv($"\ufefftag_id;nome\r\n{tag.Id};\"Nome; com \"\"aspas\"\"\nsegunda linha\"\r\nincompleta;\r\n"), "tags");
        await using (var db = f.Open()) { Assert.Equal("Inicial", (await db.Tags.SingleAsync()).Name); Assert.Equal(2, (await db.Revisions.SingleAsync()).Value); }
        Assert.Equal(2, plan.Rows.Count); Assert.Equal(2, plan.Rows[0].PhysicalStartLine); Assert.Equal("rejected", plan.Rows[1].Status);
        var result = await apply.ApplyAsync(plan.BatchId, plan.PlanHash); Assert.Equal(1, result.Applied); Assert.Equal(1, result.Rejected); await apply.ApplyAsync(plan.BatchId, plan.PlanHash);
        await using (var db = f.Open()) { var t = await db.Tags.SingleAsync(); Assert.Equal(group.Id, t.GroupId); Assert.Equal("Nome; com \"aspas\"\nsegunda linha", t.Name); Assert.Equal(new[] { "jogo", "livro" }, await db.TagMedia.OrderBy(m => m.MediaCode).Select(m => m.MediaCode).ToArrayAsync()); }
        var next = await preview.CreateAsync(Csv($"tag_id;nome;midias\n{tag.Id};;filme\n"), "tags"); await apply.ApplyAsync(next.BatchId, next.PlanHash);
        await using (var db = f.Open()) { Assert.Equal(3, await db.TagMedia.CountAsync()); Assert.Contains("aspas", (await db.Tags.SingleAsync()).Name); }
    }
    [Fact]
    public async Task ConflictsAndStalePreviewCannotChangeData()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tags = new TagService(f.Paths); var group = await tags.SaveGroupAsync("g", null, 0, "G"); var preview = new ImportPreviewService(f.Paths); var apply = new ImportApplyService(f.Paths);
        var plan = await preview.CreateAsync(Csv($"tag_id;nome;grupo_id;midias\nx;A;{group.Id};jogo\nx;B;{group.Id};livro\ny;AÇÃO;{group.Id};jogo\nz;ação;{group.Id};livro\nok;Livre;{group.Id};jogo\n"), "tags"); Assert.Equal(4, plan.Rows.Count(r => r.Status == "rejected"));
        await tags.SaveGroupAsync("g2", null, 0, "Outro"); var error = await Assert.ThrowsAsync<DomainException>(() => apply.ApplyAsync(plan.BatchId, plan.PlanHash)); Assert.Equal("preview_stale", error.Code); await using var db = f.Open(); Assert.Empty(await db.Tags.ToListAsync());
        await Assert.ThrowsAsync<DomainException>(() => preview.CreateAsync(Csv("tag_id;extra\nx;a\n"), "tags"));
        await Assert.ThrowsAsync<DomainException>(() => preview.CreateAsync(Csv("tag_id;tag_id\nx;x\n"), "tags"));
        await Assert.ThrowsAsync<DomainException>(() => preview.CreateAsync(Csv("tag_id;nome\nx;\"aberto\n"), "tags"));
        await Assert.ThrowsAsync<DomainException>(() => preview.CreateAsync(new MemoryStream(new byte[5 * 1024 * 1024 + 1]), "tags"));
    }
    [Fact]
    public async Task ExistingAssociationsAndIndependentValidRowsSurviveReimport()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tags = new TagService(f.Paths); var group = await tags.SaveGroupAsync("g", null, 0, "G"); var tag = await tags.SaveAsync(new("t", null, 0, group.Id, "T", ["jogo"])); var preview = new ImportPreviewService(f.Paths); var apply = new ImportApplyService(f.Paths);
        var csv = $"obra_id;tag_id\nalem-do-vale;{tag.Id}\ncidade-distante;{tag.Id}\nalem-do-vale;{tag.Id}\n";
        var plan = await preview.CreateAsync(Csv(csv), "associacoes"); var r = await apply.ApplyAsync(plan.BatchId, plan.PlanHash); Assert.Equal(1, r.Rejected);
        var again = await preview.CreateAsync(Csv(csv), "associacoes"); await apply.ApplyAsync(again.BatchId, again.PlanHash); await using var db = f.Open(); Assert.Single(await db.WorkTags.ToListAsync()); Assert.Empty(await db.Trackings.ToListAsync());
    }
}
