using System.Net;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.WebUtilities;

namespace IndicaAI.Tests.Web;

public sealed class BrowseOriginTests
{
    [Fact]
    public async Task WorkAndBatchReturnToTheFilteredList()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        await client.GetStringAsync("/Library");
        var tracking = await new TrackingService(app.Paths).AddAsync(new("watch", "mare-de-estrelas", "planned", new(null, null, false)));
        var lists = new ListService(app.Paths);
        var list = await lists.SaveAsync("create-list", null, 0, "Favoritos");
        var review = QueryHelpers.AddQueryString("/Batch/Review", "ids", tracking.TrackingId);
        tracking = await lists.SetMembershipAsync(new("add-to-list", tracking.Version), tracking.TrackingId, list.Id, true);
        var origin = $"/Library?Context=list&ListId={list.Id}&Media=filme&Search=estrelas&Mode=any&Include=state%3Afilme%2Fplanned&PageNumber=1";
        var browse = await client.GetStringAsync(origin);
        var target = WebUtility.HtmlDecode(Regex.Match(browse, "href=\"(/Works/Details\\?[^\"]+)\"").Groups[1].Value);
        Assert.Equal(origin, QueryHelpers.ParseQuery(new Uri(client.BaseAddress!, target).Query)["BrowseReturnUrl"]);
        var detail = WebUtility.HtmlDecode(await client.GetStringAsync(target));
        Assert.Contains($"href=\"{origin}\"", detail);
        var historyUrl = WebUtility.HtmlDecode(Regex.Match(detail, "href=\"(/Works/History\\?[^\"]+)\"").Groups[1].Value);
        var history = await client.GetStringAsync(historyUrl);
        var cycleUrl = WebUtility.HtmlDecode(Regex.Match(history, "href=\"(/Works/NewCycle\\?[^\"]+)\"").Groups[1].Value);
        Assert.Equal(origin, QueryHelpers.ParseQuery(new Uri(client.BaseAddress!, cycleUrl).Query)["BrowseReturnUrl"]);
        var batch = WebUtility.HtmlDecode(await client.GetStringAsync(QueryHelpers.AddQueryString(review, "BrowseReturnUrl", origin)));
        Assert.Contains($"href=\"{origin}\"", batch);
        Assert.Contains("Favoritos", batch);
        Assert.Equal(origin, await ApplyList(client, review, list.Id, tracking.TrackingId, tracking.Version, origin));
        const string mediaOrigin = "/Library?Media=filme&QuickState=planned";
        var mediaBatch = WebUtility.HtmlDecode(await client.GetStringAsync(QueryHelpers.AddQueryString(review, "BrowseReturnUrl", mediaOrigin)));
        Assert.Contains($"href=\"{mediaOrigin}\"", mediaBatch);
        var listOrigin = $"/Library?Context=list&ListId={list.Id}";
        var listBatch = WebUtility.HtmlDecode(await client.GetStringAsync(QueryHelpers.AddQueryString(review, "BrowseReturnUrl", listOrigin)));
        Assert.Contains("Favoritos", listBatch);
        var survivingPage = await client.GetStringAsync(listOrigin + "&PageNumber=2");
        Assert.Contains("class=\"work-card\"", survivingPage);
        Assert.Contains("PageNumber%3D1", survivingPage, StringComparison.OrdinalIgnoreCase);

        var forged = WebUtility.HtmlDecode(await client.GetStringAsync(
            QueryHelpers.AddQueryString("/Works/Details?id=mare-de-estrelas", "BrowseReturnUrl", "//example.invalid/")));
        Assert.Contains("href=\"/Library\"", forged);
        Assert.DoesNotContain("example.invalid", forged);
    }

    [Fact]
    public async Task CatalogActionAndAnimeCriteriaRespectTheMediaAndClearPreservesTheSearch()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient();
        var games = WebUtility.HtmlDecode(await client.GetStringAsync("/Library?Media=jogo&Search=vento&Include=anime%3Atrue"));
        Assert.Contains("href=\"/Catalog?Media=jogo\"", games);
        Assert.DoesNotContain("value=\"anime:true\"", games);
        await using var db = new AppDbContext(app.Paths);
        db.Works.Add(new Work { Id = "without-cover", MediaCode = "jogo", Title = "Teste sem capa" });
        await db.SaveChangesAsync();
        var gameCatalog = await client.GetStringAsync("/Catalog?Media=jogo");
        Assert.DoesNotContain("href=\"/Catalog/Tmdb?", gameCatalog);
        Assert.Contains("class=\"cover-wrap cover-missing\"", gameCatalog);
        var movies = WebUtility.HtmlDecode(await client.GetStringAsync("/Catalog?Media=filme&Search=estrelas&Include=anime%3Atrue"));
        Assert.Contains("value=\"anime:true\"", movies);
        Assert.Contains("href=\"/Catalog/Tmdb?", movies);
        Assert.Contains("href=\"/Catalog?Media=filme&Search=estrelas\"", movies);
    }

    private static async Task<string?> ApplyList(HttpClient client, string review, string listId, string trackingId,
        long version, string? origin)
    {
        var html = await client.GetStringAsync(review);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        var response = await client.PostAsync("/Batch/Review", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token), ["OperationId"] = Guid.NewGuid().ToString(),
            ["Items[0].TrackingId"] = trackingId, ["Items[0].ExpectedVersion"] = version.ToString(),
            ["ActionKind"] = "lists", ["ListId"] = listId, ["Add"] = "true", ["BrowseReturnUrl"] = origin ?? ""
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return response.Headers.Location?.OriginalString;
    }
}
