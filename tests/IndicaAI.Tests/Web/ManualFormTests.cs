using System.Net;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Web;

public class ManualFormTests
{
    private const string BookUrl = "/Works/Details?id=cidade-distante";
    private static string Token(string html)
    {
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token); return token;
    }

    [Theory] // TM15: proteção do servidor, mesmo burlando required do navegador
    [InlineData("")]
    [InlineData("watching")]
    public async Task MissingOrInvalidStateCannotCreateTracking(string state)
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var html = await client.GetStringAsync(BookUrl);
        using var response = await client.PostAsync(BookUrl + "&handler=Save", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["__RequestVerificationToken"] = Token(html), ["Input.OperationId"] = "invalid", ["Input.StateCode"] = state }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("role=\"alert\"", await response.Content.ReadAsStringAsync());
        await using var db = new AppDbContext(app.Paths);
        Assert.Empty(await db.Trackings.ToArrayAsync()); Assert.Empty(await db.Cycles.ToArrayAsync()); Assert.Empty(await db.Events.ToArrayAsync());
    }

    [Fact] // TM18
    public async Task StaleFormKeepsDraftAndCannotOverwriteOtherTab()
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        await client.GetAsync(BookUrl); var service = new TrackingService(app.Paths);
        var t = await service.AddAsync(new("add", "cidade-distante", "in_progress", new(null, null, false), "Original"));
        var staleForm = await client.GetStringAsync(BookUrl);
        await service.SaveNoteAsync(new(new("tab-b", t.Version), t.TrackingId, "Nota da aba B"));
        using var response = await client.PostAsync(BookUrl + "&handler=Save", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["__RequestVerificationToken"] = Token(staleForm), ["Input.OperationId"] = "tab-a", ["Input.Version"] = t.Version.ToString(), ["Input.StateCode"] = "in_progress", ["Input.Note"] = "Rascunho da aba A" }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("role=\"alert\"", html); Assert.Contains("Rascunho da aba A", html);
        await using var db = new AppDbContext(app.Paths); var saved = await db.Trackings.SingleAsync();
        Assert.Equal("Nota da aba B", saved.Note); Assert.Equal(t.Version + 1, saved.Version); Assert.Single(await db.Events.ToArrayAsync());
    }

    [Theory] // TM23–TM26: cada mídia usa seu próprio formulário e acompanhamento
    [InlineData("alem-do-vale", "jogo")]
    [InlineData("jardins-de-inverno", "manga")]
    [InlineData("mare-de-estrelas", "filme")]
    [InlineData("rota-do-norte", "serie")]
    public async Task OtherMediaFormsCreateExactlyOneCompatibleTracking(string work, string media)
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var url = "/Works/Details?id=" + work;
        var html = await client.GetStringAsync(url);
        using var response = await client.PostAsync(url + "&handler=Save", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["__RequestVerificationToken"] = Token(html), ["Input.OperationId"] = "add", ["Input.StateCode"] = "planned" }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var db = new AppDbContext(app.Paths); var t = await db.Trackings.SingleAsync();
        Assert.Equal(work, t.WorkId); Assert.Equal(media, t.MediaCode); Assert.Equal("planned", t.StateCode);
        Assert.Single(await db.Cycles.ToArrayAsync()); Assert.Single(await db.Events.ToArrayAsync());
    }
}
