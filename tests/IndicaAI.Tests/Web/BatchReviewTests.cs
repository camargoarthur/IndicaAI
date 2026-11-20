using System.Net;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Web;

public class BatchReviewTests
{
    [Fact]
    public async Task RemovingAGroupKeepsOtherChoicesAndDoesNotWrite()
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(new() { AllowAutoRedirect = false }); await client.GetAsync("/Library"); var service = new TrackingService(app.Paths); var works = new[] { "alem-do-vale", "cidade-distante", "rota-do-norte" }; var items = new List<TrackingResult>(); foreach (var work in works) items.Add(await service.AddAsync(new(work, work, "planned", new(null, null, false)))); var html = await client.GetStringAsync("/Batch/Review?" + string.Join('&', items.Select(i => "ids=" + i.TrackingId))); var token = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var values = new Dictionary<string, string> { ["__RequestVerificationToken"] = token, ["OperationId"] = "remove", ["media"] = "serie", ["Targets[jogo]"] = "completed", ["Targets[livro]"] = "paused" }; for (var i = 0; i < items.Count; i++) { values[$"Items[{i}].TrackingId"] = items[i].TrackingId; values[$"Items[{i}].ExpectedVersion"] = "1"; }
        var response = await client.PostAsync("/Batch/Review?handler=Remove", new FormUrlEncodedContent(values)); Assert.Equal(HttpStatusCode.OK, response.StatusCode); var body = await response.Content.ReadAsStringAsync(); Assert.DoesNotContain("Rota do norte", body); Assert.Matches("name=\"Targets\\[jogo\\]\"[\\s\\S]+?value=\"completed\" selected", body); await using var db = new AppDbContext(app.Paths); Assert.Equal(3, await db.Trackings.CountAsync(t => t.StateCode == "planned")); Assert.Equal(3, await db.Events.CountAsync());
    }
}
