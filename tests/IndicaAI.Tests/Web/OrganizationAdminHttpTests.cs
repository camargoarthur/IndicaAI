using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Archive;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Localization;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Web;

public class OrganizationAdminHttpTests
{
    [Fact]
    public async Task AssociationPickerShowsOnlyTagsForTheSelectedWorkAndGuidesAnEmptyMedia()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        const string path = "/Admin/Associations";
        await client.GetStringAsync(path);
        var tags = new TagService(app.Paths);
        var group = await tags.SaveGroupAsync("association-group", null, 0, "Gênero");
        var game = await tags.SaveAsync(new("game-tag", null, 0, group.Id, "Aventura de jogo", ["jogo"]));
        var book = await tags.SaveAsync(new("book-tag", null, 0, group.Id, "Drama de livro", ["livro"]));
        var mixed = await tags.SaveAsync(new("mixed-tag", null, 0, group.Id, "Mistério", ["jogo", "livro"]));

        var gamePage = await client.GetStringAsync(path + "?workId=alem-do-vale");
        Assert.Equal(new[] { "", game.Id, mixed.Id }, TagOptions(gamePage));
        var bookPage = await client.GetStringAsync(path + "?workId=cidade-distante");
        Assert.Equal(new[] { "", book.Id, mixed.Id }, TagOptions(bookPage));

        var moviePage = await client.GetStringAsync(path + "?workId=mare-de-estrelas");
        Assert.Equal(new[] { "" }, TagOptions(moviePage));
        Assert.Contains("/Admin/Tags#create-tag", moviePage);
        Assert.Contains(UiMessages.Lookup("AssociationNoApplicableTags", "pt-BR"), WebUtility.HtmlDecode(moviePage));
        Assert.Matches("""<button[^>]+(?=[^>]*data-association-save)(?=[^>]*disabled)[^>]*>""", moviePage);

        using var missingTag = await Post(client, path, gamePage, new()
        {
            ["operationId"] = "empty-association", ["add"] = "true", ["workId"] = "alem-do-vale"
        });
        Assert.Equal(HttpStatusCode.OK, missingTag.StatusCode);
        var message = UiMessages.Lookup("error.association_selection_required", "pt-BR");
        Assert.Contains(message, WebUtility.HtmlDecode(await missingTag.Content.ReadAsStringAsync()));

        using var incompatible = await Post(client, path, gamePage, new()
        {
            ["operationId"] = "wrong-media-association", ["add"] = "true",
            ["workId"] = "alem-do-vale", ["tagId"] = book.Id
        });
        Assert.Equal(HttpStatusCode.OK, incompatible.StatusCode);
        Assert.Contains(UiMessages.Lookup("error.incompatible", "pt-BR"), WebUtility.HtmlDecode(await incompatible.Content.ReadAsStringAsync()));

        await using var db = new AppDbContext(app.Paths);
        Assert.Empty(await db.WorkTags.ToListAsync());
        using var compatible = await Post(client, path, gamePage, new()
        {
            ["operationId"] = "valid-association", ["add"] = "true",
            ["workId"] = "alem-do-vale", ["tagId"] = game.Id
        });
        Assert.Equal(HttpStatusCode.Redirect, compatible.StatusCode);
        Assert.Equal(game.Id, (await db.WorkTags.SingleAsync()).TagId);
    }

    [Fact]
    public async Task ImportResultUsesPhysicalLinesAndOffersARejectionsViewWithDetails()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        const string previewPath = "/Admin/Import/Preview";
        var entry = await client.GetStringAsync(previewPath);
        var group = await new TagService(app.Paths).SaveGroupAsync("result-group", null, 0, "Gênero");
        var csv = $"tag_id;nome;grupo_id;midias\nvalid-tag;\"Boa\ntag\";{group.Id};jogo\n\ninvalid-tag;Drama;missing-group;jogo\n";
        using var upload = new MultipartFormDataContent();
        upload.Add(new StringContent(Token(entry)), "__RequestVerificationToken");
        upload.Add(new StringContent("tags"), "Type");
        upload.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)), "Upload", "tags.csv");
        using var created = await client.PostAsync(previewPath, upload);
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        var preview = await client.GetStringAsync(created.Headers.Location);
        Assert.Contains("linha 5", preview);
        Assert.Contains("<code>missing-group</code>", preview);
        await using var db = new AppDbContext(app.Paths);
        var batch = await db.ImportBatches.SingleAsync();
        using var applied = await Post(client, previewPath + "?handler=Apply", preview, new()
        {
            ["batchId"] = batch.Id, ["planHash"] = batch.PlanHash
        });
        Assert.Equal(HttpStatusCode.Redirect, applied.StatusCode);
        var result = await client.GetStringAsync(applied.Headers.Location);
        Assert.Contains("linha 5", result);
        Assert.Contains("<code>missing-group</code>", result);
        Assert.Contains("<code>valid-tag</code>", result);
        Assert.Contains("<code>invalid-tag</code>", result);
        Assert.Contains("Rejeitadas: 1", WebUtility.HtmlDecode(result));
        Assert.DoesNotContain("Rejeitada · Rejeitada", WebUtility.HtmlDecode(result));
        var rejectedUrl = LinkWithLabel(result, "Ver rejeições");
        var rejected = await client.GetStringAsync(rejectedUrl);
        Assert.Contains("linha 5", rejected);
        Assert.Contains("<code>missing-group</code>", rejected);
        Assert.Contains("<code>invalid-tag</code>", rejected);
        Assert.DoesNotContain("<code>valid-tag</code>", rejected);
        Assert.Equal(applied.Headers.Location!.OriginalString, LinkWithLabel(rejected, "Ver todos os registros"));
    }

    [Fact]
    public async Task RestoringShowsTheWorkTitleAndOpensTheRestoredTracking()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        await client.GetStringAsync("/Library");
        var tracking = await new TrackingService(app.Paths).AddAsync(new("archive-seed", "cidade-distante", "planned", new(null, null, false), "Minha anotação"));
        var archived = await new ArchiveService(app.Paths).SetAsync(new("archive-work", tracking.Version), tracking.TrackingId, true);
        await using (var db = new AppDbContext(app.Paths))
        {
            var work = await db.Works.SingleAsync(work => work.Id == "cidade-distante");
            work.CoverAsset = null;
            await db.SaveChangesAsync();
        }

        var archive = await client.GetStringAsync("/Archive");
        Assert.Contains("cover-missing", archive);
        Assert.Contains(UiMessages.Lookup("CoverUnavailable", "pt-BR"), WebUtility.HtmlDecode(archive));
        using var response = await Post(client, "/Archive?handler=Set", archive, new()
        {
            ["operationId"] = "restore-work", ["trackingId"] = tracking.TrackingId, ["version"] = archived.Version.ToString()
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var restored = WebUtility.HtmlDecode(await client.GetStringAsync(response.Headers.Location));
        var status = Regex.Match(restored, """<[^>]+role="status"[^>]*>(.*?)</[^>]+>""", RegexOptions.Singleline).Groups[1].Value;
        Assert.Contains("A cidade distante", status);
        Assert.DoesNotContain("class=\"work-card\"", restored);
        var workUrl = LinkWithLabel(restored, "Abrir obra restaurada");
        Assert.Equal("cidade-distante", QueryHelpers.ParseQuery(new Uri(client.BaseAddress!, workUrl).Query)["id"]);
        Assert.Contains("Minha anotação", WebUtility.HtmlDecode(await client.GetStringAsync(workUrl)));
        Assert.DoesNotContain("Abrir obra restaurada", WebUtility.HtmlDecode(await client.GetStringAsync("/Archive")));
    }

    [Fact]
    public async Task ListsPrioritizeOpeningAndCountTheWorksShownInTheList()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient();
        await client.GetStringAsync("/Library");
        var lists = new ListService(app.Paths);
        var list = await lists.SaveAsync("count-list", null, 0, "Fim de semana");
        var emptyList = await lists.SaveAsync("empty-count-list", null, 0, "Vazia");
        var tracking = new TrackingService(app.Paths);
        var game = await tracking.AddAsync(new("seed-game", "alem-do-vale", "planned", new(null, null, false)));
        var book = await tracking.AddAsync(new("seed-book", "cidade-distante", "planned", new(null, null, false)));
        await lists.SetMembershipAsync(new("game-member", game.Version), game.TrackingId, list.Id, true);
        var member = await lists.SetMembershipAsync(new("book-member", book.Version), book.TrackingId, list.Id, true);
        var archive = new ArchiveService(app.Paths);
        var archived = await archive.SetAsync(new("hide-book", member.Version), book.TrackingId, true);

        var overview = WebUtility.HtmlDecode(await client.GetStringAsync("/Lists"));
        var card = ListCard(overview, "Fim de semana");
        Assert.Contains(UiMessages.Lookup("WorkCount", "pt-BR", 1), card);
        var path = LinkWithLabel(card, "Abrir lista");
        Assert.Contains(list.Id, path);
        Assert.True(card.IndexOf(path, StringComparison.Ordinal) < card.IndexOf("<summary", StringComparison.Ordinal));
        Assert.Matches("""<details>\s*<summary[^>]*>Renomear</summary>\s*<form""", card);
        var emptyCard = ListCard(overview, "Vazia");
        Assert.Contains(UiMessages.Lookup("WorkCount", "pt-BR", 0), emptyCard);
        Assert.Contains(emptyList.Id, LinkWithLabel(emptyCard, "Abrir lista"));
        var opened = WebUtility.HtmlDecode(await client.GetStringAsync(path));
        Assert.Contains("Além do vale", opened);
        Assert.DoesNotContain("A cidade distante", opened);

        await archive.SetAsync(new("restore-book", archived.Version), book.TrackingId, false);
        var afterRestore = WebUtility.HtmlDecode(await client.GetStringAsync("/Lists"));
        Assert.Contains(UiMessages.Lookup("WorkCount", "pt-BR", 2), ListCard(afterRestore, "Fim de semana"));
    }

    private static string[] TagOptions(string html)
    {
        var select = Regex.Match(html, """<select\b[^>]*id="association-tag"[^>]*>(.*?)</select>""", RegexOptions.Singleline);
        return Regex.Matches(select.Groups[1].Value, """<option\b[^>]*value="([^"]*)["]""")
            .Select(option => option.Groups[1].Value).ToArray();
    }

    private static string ListCard(string html, string name) =>
        Assert.Single(Regex.Matches(html, """<section\b[^>]*>(.*?)</section>""", RegexOptions.Singleline)
            .Where(card => card.Value.Contains($">{name}</h2>"))).Value;

    private static string LinkWithLabel(string html, string label)
    {
        var link = Assert.Single(Regex.Matches(html, """<a\b([^>]*)>(.*?)</a>""", RegexOptions.Singleline)
            .Where(candidate => WebUtility.HtmlDecode(candidate.Groups[2].Value).Trim() == label));
        return WebUtility.HtmlDecode(Regex.Match(link.Groups[1].Value, """href="([^"]+)["]""").Groups[1].Value);
    }

    private static string Token(string html) => Regex.Match(html, """name="__RequestVerificationToken" type="hidden" value="([^"]+)["]""").Groups[1].Value;

    private static Task<HttpResponseMessage> Post(HttpClient client, string path, string html, Dictionary<string, string> fields)
    {
        fields["__RequestVerificationToken"] = Token(html);
        return client.PostAsync(path, new FormUrlEncodedContent(fields));
    }
}
