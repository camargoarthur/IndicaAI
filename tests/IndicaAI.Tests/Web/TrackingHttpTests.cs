using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using IndicaAI.Web.Persistence;
namespace IndicaAI.Tests.Web;

public sealed class TrackingHttpTests
{
    [Theory]
    [InlineData("")]
    [InlineData("nota pessoal")]
    public async Task AddFromFormRequiresStateAndStoresAnActualTracking(string note)
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var html = await client.GetStringAsync("/Works/Details?id=cidade-distante");
        Assert.Contains("Meu acompanhamento", html);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        var data = new Dictionary<string, string> { ["__RequestVerificationToken"] = token, ["Input.OperationId"] = "web-add", ["Input.StateCode"] = "in_progress", ["Input.Note"] = note };
        var action = WebUtility.HtmlDecode(Regex.Match(html, "action=\"([^\"]*handler=Save[^\"]*)\"").Groups[1].Value);
        Assert.Contains("id=cidade-distante", action, StringComparison.OrdinalIgnoreCase);
        var saved = await client.PostAsync(action, new FormUrlEncodedContent(data));
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        await using var db = new AppDbContext(app.Paths);
        Assert.Equal(note, (await db.Trackings.SingleAsync()).Note);
        Assert.Equal(1, await db.Cycles.CountAsync());
        Assert.Equal(1, await db.Events.CountAsync());
        var withoutToken = await client.PostAsync("/Works/Details?id=cidade-distante&handler=Save", new FormUrlEncodedContent(data.Where(p => p.Key != "__RequestVerificationToken")));
        Assert.Equal(HttpStatusCode.BadRequest, withoutToken.StatusCode);
    }
}
