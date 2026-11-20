using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Settings;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Web;

public class JourneyTests
{
    static string Token(string html) => Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
    [Fact]
    public async Task RealFormsOrganizeBatchArchiveAndImportWithoutLogin()
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        async Task Post(string url, Dictionary<string, string> values, string get) { values["__RequestVerificationToken"] = Token(await client.GetStringAsync(get)); Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(url, new FormUrlEncodedContent(values))).StatusCode); }
        foreach (var work in new[] { "alem-do-vale", "cidade-distante" }) await Post("/Works/Details?id=" + work + "&handler=Save", new() { ["Input.OperationId"] = "journey-" + work, ["Input.StateCode"] = "planned", ["Input.Note"] = "<script>alert('x')</script>" }, "/Works/Details?id=" + work);
        await Post("/Lists", new() { ["OperationId"] = "jl", ["Name"] = "Minhas leituras e jogos" }, "/Lists");
        await using var db = new AppDbContext(app.Paths); var list = await db.Lists.SingleAsync(); var trackings = await db.Trackings.OrderBy(t => t.Id).ToArrayAsync();
        var values = new Dictionary<string, string> { ["OperationId"] = "batch-list", ["ActionKind"] = "lists", ["ListId"] = list.Id, ["Add"] = "true" }; for (var i = 0; i < trackings.Length; i++) { values[$"Items[{i}].TrackingId"] = trackings[i].Id; values[$"Items[{i}].ExpectedVersion"] = "1"; }
        await Post("/Batch/Review", values, "/Batch/Review?" + string.Join('&', trackings.Select(t => "ids=" + t.Id))); Assert.Equal(2, await db.ListItems.CountAsync());
        values["OperationId"] = "batch-state"; values["ActionKind"] = "states"; values["Targets[jogo]"] = "in_progress"; values["Targets[livro]"] = "completed"; for (var i = 0; i < trackings.Length; i++) values[$"Items[{i}].ExpectedVersion"] = "2"; await Post("/Batch/Review", values, "/Library");
        var first = trackings[0]; await Post("/Archive?handler=Set", new() { ["operationId"] = "jarc", ["trackingId"] = first.Id, ["version"] = "3", ["archived"] = "true" }, "/Archive");
        await Post("/Archive?handler=Set", new() { ["operationId"] = "jrestore", ["trackingId"] = first.Id, ["version"] = "4", ["archived"] = "false" }, "/Archive");
        var html = await client.GetStringAsync("/Works/Details?id=" + first.WorkId); Assert.Contains("&lt;script&gt;", html); Assert.DoesNotContain("<script>alert", html);
        await Post("/Admin/Tags?kind=group", new() { ["OperationId"] = "jg", ["Name"] = "Gênero" }, "/Admin/Tags"); var group = await db.Groups.SingleAsync();
        using var content = new MultipartFormDataContent(); content.Add(new StringContent(Token(await client.GetStringAsync("/Admin/Import/Preview"))), "__RequestVerificationToken"); content.Add(new StringContent("tags"), "Type"); content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes($"tag_id;nome;grupo_id;midias\nj-http;Aventura;{group.Id};jogo\n")), "Upload", "tags.csv"); var uploaded = await client.PostAsync("/Admin/Import/Preview", content); Assert.Equal(HttpStatusCode.Redirect, uploaded.StatusCode); var batch = await db.ImportBatches.SingleAsync(); await Post("/Admin/Import/Preview?handler=Apply", new() { ["batchId"] = batch.Id, ["planHash"] = batch.PlanHash }, uploaded.Headers.Location!.ToString()); Assert.True(await db.Tags.AnyAsync(t => t.Id == "j-http"));
        foreach (var url in new[] { "/Library", "/Catalog", "/Lists", "/Archive", "/Settings", "/Admin/Tags", "/Admin/Associations", "/Works/History?id=" + first.Id, "/Admin/Import/Result?id=" + batch.Id }) { var page = await client.GetStringAsync(url); Assert.DoesNotContain("Mensagem indispon", page); Assert.DoesNotContain("type=\"password\"", page); }
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Export")).StatusCode);
        using var external = new HttpRequestMessage(HttpMethod.Get, "/Library"); external.Headers.Host = "attacker.example"; Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(external)).StatusCode);
        using var origin = new HttpRequestMessage(HttpMethod.Post, "/Settings?handler=Quick"); origin.Headers.Add("Origin", "https://attacker.example"); Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(origin)).StatusCode);
    }
    [Fact]
    public async Task FourCombinationsRenderLanguageAndThemeWithStableCanonicalValues()
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(); await client.GetAsync("/Library"); var preferences = new PreferenceService(app.Paths);
        foreach (var theme in new[] { "light", "dark" }) foreach (var locale in new[] { "pt-BR", "en" }) { var p = await preferences.GetAsync(); await preferences.SetAsync(new(Guid.NewGuid().ToString(), p.Version), theme, locale); foreach (var path in new[] { "/Library", "/Works/Details?id=cidade-distante", "/Admin/Tags", "/Settings" }) { var html = await client.GetStringAsync(path); Assert.Contains("lang=\"" + locale + "\"", html); Assert.Contains("data-theme=\"" + theme + "\"", html); Assert.DoesNotContain("Mensagem indispon", html); Assert.Contains("cidade-distante", path.Contains("Details") ? html : "cidade-distante"); } }
    }
}

