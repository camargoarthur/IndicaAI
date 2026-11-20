using System.Text;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Archive;
using IndicaAI.Web.Features.Batch;
using IndicaAI.Web.Features.Imports;
using IndicaAI.Web.Features.Library;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Features.Tracking;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Integration;

public class ManualGuideTests
{
    [Fact] // TM05
    public async Task RenamingAListPreservesMembershipAndRejectsStaleEdits()
    {
        var f = new TestDatabase(); await f.InitializeAsync();
        var lists = new ListService(f.Paths);
        var t = await new TrackingService(f.Paths).AddAsync(new("add", "cidade-distante", "planned", new(null, null, false)));
        var list = await lists.SaveAsync("create", null, 0, "QA Revisar");
        await lists.SetMembershipAsync(new("member", t.Version), t.TrackingId, list.Id, true);
        var renamed = await lists.SaveAsync("rename", list.Id, list.Version, "QA Revisões");
        Assert.Equal(list.Id, renamed.Id);
        var error = await Assert.ThrowsAsync<DomainException>(() => lists.SaveAsync("stale", list.Id, list.Version, "Perdido"));
        Assert.Equal("conflict", error.Code);
        await Assert.ThrowsAsync<DomainException>(() => lists.SaveAsync("blank", null, 0, "   "));
        await using var db = f.Open();
        Assert.Equal("QA Revisões", (await db.Lists.SingleAsync()).Name);
        Assert.Equal(list.Id, (await db.ListItems.SingleAsync()).ListId);
        Assert.Equal("planned", (await db.Trackings.SingleAsync()).StateCode);
    }

    [Fact] // TM27–TM29
    public async Task TagAndOrNotFiltersRespectMediaAndListContext()
    {
        var f = new TestDatabase(); await f.InitializeAsync();
        var tracking = new TrackingService(f.Paths); var tags = new TagService(f.Paths);
        var group = await tags.SaveGroupAsync("group", null, 0, "QA Temas");
        var explore = await tags.SaveAsync(new("explore", null, 0, group.Id, "Exploração", MediaCodes.All));
        var drama = await tags.SaveAsync(new("drama", null, 0, group.Id, "Drama", ["livro", "filme"]));
        var records = new Dictionary<string, TrackingResult>();
        foreach (var work in new[] { "alem-do-vale", "cidade-distante", "mare-de-estrelas" })
            records[work] = await tracking.AddAsync(new("add-" + work, work, "planned", new(null, null, false)));
        foreach (var work in new[] { "alem-do-vale", "cidade-distante" }) await tags.AssociateAsync("explore-" + work, work, explore.Id);
        foreach (var work in new[] { "cidade-distante", "mare-de-estrelas" }) await tags.AssociateAsync("drama-" + work, work, drama.Id);
        var query = new LibraryQuery(f.Paths);
        var filter = new LibraryFilter { Include = [new("tag", explore.Id), new("tag", drama.Id)] };
        Assert.Equal(["cidade-distante"], (await query.ExecuteAsync(filter)).Items.Select(i => i.Work.Id).ToArray());
        Assert.Equal(3, (await query.ExecuteAsync(filter with { Mode = "any" })).Total);
        Assert.Equal(["alem-do-vale"], (await query.ExecuteAsync(filter with { Include = [new("tag", explore.Id)], Exclude = [new("tag", drama.Id)] })).Items.Select(i => i.Work.Id).ToArray());
        Assert.Equal(["cidade-distante"], (await query.ExecuteAsync(filter with { Mode = "any", Media = "livro" })).Items.Select(i => i.Work.Id).ToArray());
        var lists = new ListService(f.Paths); var list = await lists.SaveAsync("list", null, 0, "QA Favoritos");
        var book = records["cidade-distante"];
        await lists.SetMembershipAsync(new("membership", book.Version), book.TrackingId, list.Id, true);
        Assert.Equal(["cidade-distante"], (await query.ExecuteAsync(filter with { Mode = "any", Context = "list", ListId = list.Id })).Items.Select(i => i.Work.Id).ToArray());
    }

    [Fact] // TM22, TM27, TM29
    public async Task SearchAnimeAndArchiveStayInsideTheirContexts()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tracking = new TrackingService(f.Paths);
        var anime = await tracking.AddAsync(new("anime", "rota-do-norte", "in_progress", new(null, null, false)));
        await tracking.AddAsync(new("book", "cidade-distante", "planned", new(null, null, false)));
        var query = new LibraryQuery(f.Paths);
        Assert.Equal(["cidade-distante"], (await query.ExecuteAsync(new() { Search = "  CIDADE DISTANTE  " })).Items.Select(i => i.Work.Id).ToArray());
        Assert.Empty((await query.ExecuteAsync(new() { Search = "inexistente" })).Items);
        var filter = new LibraryFilter { Include = [new("anime", "true")] };
        Assert.Equal(["rota-do-norte"], (await query.ExecuteAsync(filter)).Items.Select(i => i.Work.Id).ToArray());
        Assert.Empty((await query.ExecuteAsync(filter with { Media = "livro" })).Items);
        await new ArchiveService(f.Paths).SetAsync(new("archive", anime.Version), anime.TrackingId, true);
        Assert.Empty((await query.ExecuteAsync(filter)).Items);
        Assert.Equal(1, (await query.ExecuteAsync(filter with { Context = "archived" })).Total);
        Assert.Equal(1, (await query.ExecuteAsync(filter with { Context = "catalog" })).Total);
        Assert.Equal(5, (await query.ExecuteAsync(new() { Context = "catalog" })).Total);
    }

    [Fact] // TM34
    public async Task BatchListRemovalKeepsOtherMembershipsAndConsumptionHistory()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tracking = new TrackingService(f.Paths);
        var book = await tracking.AddAsync(new("book", "cidade-distante", "planned", new(null, null, false), "Privada"));
        var game = await tracking.AddAsync(new("game", "alem-do-vale", "in_progress", new(null, null, false)));
        var lists = new ListService(f.Paths); var a = await lists.SaveAsync("a", null, 0, "Favoritos"); var b = await lists.SaveAsync("b", null, 0, "Revisões");
        var batch = new BatchService(f.Paths);
        await batch.ChangeListsAsync("add-a", [new(book.TrackingId, 1), new(game.TrackingId, 1)], a.Id, true);
        await batch.ChangeListsAsync("add-b", [new(book.TrackingId, 2), new(game.TrackingId, 2)], b.Id, true);
        await batch.ChangeListsAsync("remove", [new(game.TrackingId, 3)], b.Id, false);
        await batch.ChangeListsAsync("remove", [new(game.TrackingId, 3)], b.Id, false);
        await using var db = f.Open();
        Assert.Equal(3, await db.ListItems.CountAsync());
        Assert.False(await db.ListItems.AnyAsync(i => i.TrackingId == game.TrackingId && i.ListId == b.Id));
        Assert.Equal(2, await db.Events.CountAsync());
        Assert.Equal("Privada", (await db.Trackings.FindAsync(book.TrackingId))!.Note);
        Assert.Equal("in_progress", (await db.Trackings.FindAsync(game.TrackingId))!.StateCode);
    }

    [Fact] // TM11
    public async Task CancelledCsvCannotApplyAndDoesNotChangeGlobalData()
    {
        var f = new TestDatabase(); await f.InitializeAsync();
        var group = await new TagService(f.Paths).SaveGroupAsync("group", null, 0, "QA Temas");
        using var csv = new MemoryStream(Encoding.UTF8.GetBytes($"tag_id;nome;grupo_id;midias\nqa;Fantasia;{group.Id};livro\n"));
        var plan = await new ImportPreviewService(f.Paths).CreateAsync(csv, "tags");
        var apply = new ImportApplyService(f.Paths);
        await apply.CancelAsync(plan.BatchId);
        var result = await apply.ApplyAsync(plan.BatchId, plan.PlanHash);
        Assert.Equal("cancelled", result.Status); Assert.Equal(0, result.Applied);
        await using var db = f.Open(); Assert.Empty(await db.Tags.ToArrayAsync()); Assert.Empty(await db.WorkTags.ToArrayAsync());
        Assert.Equal(1, (await db.Revisions.SingleAsync()).Value);
    }

    [Fact] // TM21
    public async Task CorrectingAnOldCycleKeepsCurrentCycleAndState()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tracking = new TrackingService(f.Paths);
        var original = await tracking.AddAsync(new("add", "cidade-distante", "completed", new(new(2020, 1, 1), new(2020, 2, 1), false)));
        string oldId; await using (var db = f.Open()) oldId = (await db.Cycles.SingleAsync()).Id;
        var current = await tracking.StartCycleAsync(new(new("cycle", original.Version), original.TrackingId, "in_progress", new(null, null, false)));
        await tracking.CorrectDateAsync(new(new("correct", current.Version), original.TrackingId, oldId, "start", new(2019, 12, 1), false));
        await using var read = f.Open();
        var cycles = await read.Cycles.OrderBy(c => c.Ordinal).ToArrayAsync();
        Assert.Equal(new DateOnly(2019, 12, 1), cycles[0].StartOn); Assert.Null(cycles[1].StartOn);
        Assert.Equal("in_progress", (await read.Trackings.SingleAsync()).StateCode);
        var correction = await read.Events.SingleAsync(e => e.Type == "date_corrected");
        Assert.Equal(oldId, correction.CycleId); Assert.Equal("2020-01-01", correction.OldValue); Assert.Equal("2019-12-01", correction.NewValue);
        Assert.NotNull(correction.CorrectedEventId);
    }

    [Fact] // TM06, TM08–TM10
    public async Task TagRenameAndAssociationRemovalPreserveTracking()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var tags = new TagService(f.Paths);
        var group = await tags.SaveGroupAsync("group", null, 0, "QA Temas");
        await Assert.ThrowsAsync<DomainException>(() => tags.SaveAsync(new("no-media", null, 0, group.Id, "Inválida", [])));
        var tag = await tags.SaveAsync(new("tag", null, 0, group.Id, "QA Só livro", ["livro"]));
        await tags.AssociateAsync("link", "cidade-distante", tag.Id);
        await new TrackingService(f.Paths).AddAsync(new("book", "cidade-distante", "in_progress", new(null, null, false), "Minha nota"));
        var renamed = await tags.SaveAsync(new("rename", tag.Id, tag.Version, group.Id, "QA Exclusiva livro", ["livro"]));
        Assert.Equal(tag.Id, renamed.Id);
        await using (var read = f.Open()) { Assert.Single(await read.WorkTags.ToArrayAsync()); Assert.Equal("QA Exclusiva livro", (await read.Tags.SingleAsync()).Name); }
        Assert.True(await tags.AssociateAsync("remove", "cidade-distante", tag.Id, false));
        Assert.False(await tags.AssociateAsync("remove-again", "cidade-distante", tag.Id, false));
        await using var db = f.Open(); Assert.Empty(await db.WorkTags.ToArrayAsync());
        Assert.Equal("Minha nota", (await db.Trackings.SingleAsync()).Note); Assert.Single(await db.Events.ToArrayAsync());
    }
}
