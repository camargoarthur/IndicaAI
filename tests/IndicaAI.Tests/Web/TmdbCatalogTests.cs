using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Web;

public sealed class TmdbCatalogTests
{
    private const string Page = "/Catalog/Tmdb";
    private const string Preview = Page + "?handler=Preview&id=426063";
    private const string Confirm = Page + "?handler=Confirm";
    private const string FakeToken = "fake-b06-read-token";

    [Fact]
    public async Task SearchPreviewAndRepeatedImportOpenTheSameWorkAndPreserveTracking()
    {
        await using var app = new WebFixture();
        var api = new TmdbHandler();
        await using var host = Configure(app, api, FakeToken);
        using var client = host.CreateClient(new() { AllowAutoRedirect = false });
        Assert.Contains(Page, await client.GetStringAsync("/Catalog"));

        var results = await client.GetStringAsync(Page + "?Query=Nosferatu&Year=2024");
        Assert.Contains("Nosferatu", results);
        Assert.Contains("2024", results);
        Assert.Contains("https://image.tmdb.org/t/p/w342/film-poster.jpg", results);
        Assert.Contains("Sinopse no resultado", results);
        Assert.Contains("Sem p&#xF4;ster", results);
        Assert.Contains("Sinopse indispon", results);
        var search = Assert.Single(api.Requests);
        Assert.Equal("/3/search/movie", search.AbsolutePath);
        Assert.Contains("language=pt-BR", search.Query);
        Assert.Contains("primary_release_year=2024", search.Query);

        var preview = await client.GetStringAsync(Preview);
        Assert.Contains("Robert Eggers", preview);
        Assert.Contains("133", preview);
        Assert.Contains("Sinopse de teste", preview);
        Assert.DoesNotContain(FakeToken, preview);
        Assert.Equal("/3/movie/426063", api.Requests[1].AbsolutePath);
        Assert.Contains("append_to_response=credits", api.Requests[1].Query);
        await using var db = new AppDbContext(app.Paths);
        Assert.False(await db.Works.AnyAsync(w => w.Title == "Nosferatu"));

        var ticket = FormValue(preview, "PreviewTicket");
        var antiforgery = FormValue(preview, "__RequestVerificationToken");
        var invalid = await Post(client, Confirm, new() { ["PreviewTicket"] = "invalid-" + ticket }, antiforgery);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.False(await db.Works.AnyAsync(w => w.Title == "Nosferatu"));

        var added = await Post(client, Confirm, new() { ["PreviewTicket"] = ticket }, antiforgery);
        Assert.Equal(HttpStatusCode.Redirect, added.StatusCode);
        var detailsUrl = added.Headers.Location!.OriginalString;
        Assert.StartsWith("/Works/Details?id=", detailsUrl);
        var film = await db.Works.SingleAsync(w => w.Title == "Nosferatu");
        Assert.Equal("filme", film.MediaCode);
        Assert.Equal(2024, film.ReleaseYear);
        Assert.Equal(133, (await db.Films.SingleAsync(f => f.WorkId == film.Id)).DurationMinutes);
        Assert.Equal("Robert Eggers", (await db.Credits.SingleAsync(c => c.WorkId == film.Id)).Name);
        Assert.False(await db.Trackings.AnyAsync(t => t.WorkId == film.Id));

        var tracking = await Post(client, detailsUrl + "&handler=Save", new()
        {
            ["Input.OperationId"] = "tmdb-tracking",
            ["Input.StateCode"] = "planned",
            ["Input.Note"] = "Minha nota privada"
        }, FormValue(await client.GetStringAsync(detailsUrl), "__RequestVerificationToken"));
        Assert.Equal(HttpStatusCode.Redirect, tracking.StatusCode);

        var repeated = await Post(client, Confirm, new() { ["PreviewTicket"] = ticket, ["IsAnime"] = "true" }, antiforgery);
        Assert.Equal(detailsUrl, repeated.Headers.Location!.OriginalString);
        Assert.Equal(detailsUrl, (await client.GetAsync(Preview)).Headers.Location!.OriginalString);
        Assert.Equal(2, api.Requests.Count);
        Assert.False((await db.Works.AsNoTracking().SingleAsync(w => w.Id == film.Id)).IsAnime);
        Assert.Equal("Minha nota privada", (await db.Trackings.SingleAsync(t => t.WorkId == film.Id)).Note);
        Assert.Single(await db.Events.ToArrayAsync());
    }

    [Fact]
    public async Task LocalSearchHandsOffTitleWithoutCallingProviderUntilSubmitted()
    {
        await using var app = new WebFixture();
        var api = new TmdbHandler();
        await using var host = Configure(app, api, FakeToken);
        using var client = host.CreateClient();

        var local = await client.GetStringAsync("/Catalog?Media=filme&Search=Nosferatu");
        var link = Regex.Match(WebUtility.HtmlDecode(local), "href=\"([^\"]*Prefill=Nosferatu[^\"]*)\"").Groups[1].Value;
        Assert.NotEmpty(link);
        var externalForm = WebUtility.HtmlDecode(await client.GetStringAsync(link));
        Assert.Contains("value=\"Nosferatu\"", externalForm);
        Assert.Contains("href=\"/Catalog?Media=filme&Search=Nosferatu\"", externalForm);
        Assert.DoesNotContain("data-tmdb-search-completed", externalForm);
        Assert.Empty(api.Requests);

        var completed = await client.GetStringAsync(Page + "?Query=Nosferatu&Remember=true");
        Assert.Contains("data-tmdb-search-completed", completed);
        Assert.Single(api.Requests);
        var paged = await client.GetStringAsync(Page + "?Query=Nosferatu&ResultPage=2");
        Assert.DoesNotContain("data-tmdb-search-completed", paged);
        var invalid = await client.GetStringAsync(Page + "?Query=%20&Remember=true");
        Assert.DoesNotContain("data-tmdb-search-completed", invalid);
    }

    [Fact]
    public async Task CatalogCardsDistinguishActiveArchivedAndUntrackedWorks()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        const string details = "/Works/Details?id=mare-de-estrelas";
        const string catalog = "/Catalog?Media=filme";
        var notTracked = WebUtility.HtmlDecode(await client.GetStringAsync(catalog));
        Assert.Contains("Sem acompanhamento", notTracked);

        var tracking = await Post(client, details + "&handler=Save", new()
        {
            ["Input.OperationId"] = "catalog-tracking",
            ["Input.StateCode"] = "planned"
        }, FormValue(await client.GetStringAsync(details), "__RequestVerificationToken"));
        Assert.Equal(HttpStatusCode.Redirect, tracking.StatusCode);

        var active = WebUtility.HtmlDecode(await client.GetStringAsync(catalog));
        Assert.Contains("Quero assistir", active);
        Assert.DoesNotContain("Sem acompanhamento", active);
        Assert.DoesNotContain("catalog-card-archive", active);

        var trackedDetails = TrackingActionForm(await client.GetStringAsync(details));
        var archive = await Post(client, "/Archive?handler=Set", new()
        {
            ["operationId"] = "catalog-archive",
            ["trackingId"] = FormValue(trackedDetails, "trackingId"),
            ["version"] = FormValue(trackedDetails, "version"),
            ["archived"] = "true"
        }, FormValue(trackedDetails, "__RequestVerificationToken"));
        Assert.Equal(HttpStatusCode.Redirect, archive.StatusCode);
        var archived = WebUtility.HtmlDecode(await client.GetStringAsync(catalog));
        Assert.Contains("Quero assistir", archived);
        Assert.Contains("catalog-card-archive", archived);
        Assert.DoesNotContain("Sem acompanhamento", archived);

        var archivePage = TrackingActionForm(await client.GetStringAsync("/Archive"));
        var restore = await Post(client, "/Archive?handler=Set", new()
        {
            ["operationId"] = "catalog-restore",
            ["trackingId"] = FormValue(archivePage, "trackingId"),
            ["version"] = FormValue(archivePage, "version")
        }, FormValue(archivePage, "__RequestVerificationToken"));
        Assert.Equal(HttpStatusCode.Redirect, restore.StatusCode);
        var restored = WebUtility.HtmlDecode(await client.GetStringAsync(catalog));
        Assert.Contains("Quero assistir", restored);
        Assert.DoesNotContain("catalog-card-archive", restored);
    }

    private static string TrackingActionForm(string html) => Regex.Matches(html, "<form\\b[^>]*>[\\s\\S]*?</form>")
        .Select(match => match.Value).Single(form => form.Contains("name=\"trackingId\"", StringComparison.Ordinal));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfigurationAndProviderFailuresAreSafeForLocalCatalog(bool configured)
    {
        await using var app = new WebFixture();
        var api = new TmdbHandler(HttpStatusCode.Unauthorized);
        await using var host = Configure(app, api, configured ? FakeToken : "");
        using var client = host.CreateClient();
        var lookup = await client.GetStringAsync(Page + "?Query=Nosferatu&Remember=true");
        Assert.Contains(configured ? "autentica" : "Configure o token", lookup);
        Assert.DoesNotContain(FakeToken, lookup);
        Assert.DoesNotContain("data-tmdb-search-completed", lookup);
        Assert.Equal(configured ? 1 : 0, api.Requests.Count);
        Assert.Contains("A cidade distante", await client.GetStringAsync("/Catalog"));
    }

    [Theory]
    [InlineData("cross-site")]
    [InlineData("same-site")]
    public async Task ForeignBrowserContextCannotTriggerProviderRequests(string fetchSite)
    {
        await using var app = new WebFixture();
        var api = new TmdbHandler();
        await using var host = Configure(app, api, FakeToken);
        using var client = host.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, Page + "?Query=Nosferatu");
        request.Headers.Add("Sec-Fetch-Site", fetchSite);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(api.Requests);
    }

    private static WebApplicationFactory<global::Program> Configure(WebFixture app, TmdbHandler api, string token) =>
        app.WithWebHostBuilder(builder => builder.UseSetting("IndicaAI:TmdbReadToken", token)
            .ConfigureServices(services => services.AddHttpClient("Tmdb").ConfigurePrimaryHttpMessageHandler(() => api)));

    private static Task<HttpResponseMessage> Post(HttpClient client, string path, Dictionary<string, string> values, string antiforgery)
    {
        values["__RequestVerificationToken"] = antiforgery;
        return client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private static string FormValue(string html, string name)
    {
        var tags = Regex.Matches(html, "<input\\b[^>]*>").Select(match => match.Value);
        var input = tags.First(tag => tag.Contains($"name=\"{name}\"", StringComparison.Ordinal));
        return WebUtility.HtmlDecode(Regex.Match(input, "\\bvalue=\"([^\"]+)\"").Groups[1].Value);
    }

    private sealed class TmdbHandler(HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal(FakeToken, request.Headers.Authorization?.Parameter);
            var uri = request.RequestUri!;
            Requests.Add(uri);
            var json = uri.AbsolutePath == "/3/search/movie"
                ? """{"results":[null,{"id":426063,"title":"Nosferatu","original_title":"Nosferatu","release_date":"2024-12-25","overview":"Sinopse no resultado","poster_path":"/film-poster.jpg"},{"id":1922,"title":"Nosferatu","release_date":"1922-03-04"}],"total_pages":1}"""
                : """{"id":426063,"title":"Nosferatu","overview":"Sinopse de teste","release_date":"2024-12-25","runtime":133,"credits":{"crew":[null,{"name":"Robert Eggers","job":"Director"}]}}""";
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
