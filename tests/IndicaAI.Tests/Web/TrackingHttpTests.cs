using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
using Microsoft.EntityFrameworkCore;
using IndicaAI.Web.Persistence;
namespace IndicaAI.Tests.Web;

public sealed class TrackingHttpTests
{
    [Fact]
    public async Task PlannedFormPreservesStoredDatesAndAcceptsAnUnrelatedNoteEdit()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        const string path = "/Works/Details?id=cidade-distante";
        _ = await client.GetStringAsync(path);
        var tracking = await new TrackingService(app.Paths).AddAsync(new("seed-dates", "cidade-distante", "planned",
            new(new(2099, 1, 2), new(2020, 1, 2), true)));
        var html = await client.GetStringAsync(path);
        Assert.Matches("<input[^>]*name=\"Input.Start\"[^>]*disabled|<input[^>]*disabled[^>]*name=\"Input.Start\"", html);
        Assert.DoesNotContain("Manter as datas informadas", WebUtility.HtmlDecode(html));

        var result = await client.PostAsync(path + "&handler=Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(html), ["Input.OperationId"] = "edit-note",
            ["Input.Version"] = tracking.Version.ToString(), ["Input.StateCode"] = "planned",
            ["Input.Note"] = "Nota atualizada", ["Input.Completion"] = "2025-01-01"
        }));
        Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
        await using var db = new AppDbContext(app.Paths);
        var cycle = await db.Cycles.SingleAsync();
        Assert.Equal(new DateOnly(2099, 1, 2), cycle.StartOn);
        Assert.Equal(new DateOnly(2020, 1, 2), cycle.CompletionOn);
        Assert.Equal("Nota atualizada", (await db.Trackings.SingleAsync()).Note);
        Assert.Single(await db.Events.ToArrayAsync());
    }

    [Fact]
    public async Task UnusualDatesReceiveSpecificConfirmationBeforeTheyAreSaved()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        const string path = "/Works/Details?id=cidade-distante";
        var html = await client.GetStringAsync(path);
        var post = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(html), ["Input.OperationId"] = "unusual-form",
            ["Input.StateCode"] = "completed", ["Input.Start"] = "2099-01-02", ["Input.Completion"] = "2020-01-02",
            ["Input.Note"] = "Rascunho"
        };
        var first = await client.PostAsync(path + "&handler=Save", new FormUrlEncodedContent(post));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var warning = await first.Content.ReadAsStringAsync();
        var decoded = WebUtility.HtmlDecode(warning);
        Assert.Contains("A data de início está no futuro.", decoded);
        Assert.Contains("A conclusão vem antes da data de início.", decoded);
        Assert.Contains("Rascunho", decoded);
        await using var db = new AppDbContext(app.Paths);
        Assert.Empty(await db.Trackings.ToArrayAsync());
        post["__RequestVerificationToken"] = Token(warning);
        post["Input.ConfirmDates"] = "true";
        post["Input.DateConfirmationKey"] = PatternValue(warning, "Input.DateConfirmationKey");

        post["Input.Start"] = "2099-01-03";
        var changed = await client.PostAsync(path + "&handler=Save", new FormUrlEncodedContent(post));
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var nextWarning = await changed.Content.ReadAsStringAsync();
        post["__RequestVerificationToken"] = Token(nextWarning);
        post["Input.DateConfirmationKey"] = PatternValue(nextWarning, "Input.DateConfirmationKey");
        Assert.Equal(HttpStatusCode.Redirect,
            (await client.PostAsync(path + "&handler=Save", new FormUrlEncodedContent(post))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,
            (await client.PostAsync(path + "&handler=Save", new FormUrlEncodedContent(post))).StatusCode);
        Assert.Equal(new DateOnly(2099, 1, 3), (await db.Cycles.SingleAsync()).StartOn);
        Assert.Single(await db.Events.ToArrayAsync());
    }

    [Fact]
    public async Task KeepingTheCycleRetainsHiddenCompletionAndAnExplicitClearIsAudited()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        const string path = "/Works/Details?id=cidade-distante";
        _ = await client.GetStringAsync(path);
        var tracking = await new TrackingService(app.Paths).AddAsync(new("finished", "cidade-distante", "completed",
            new(new(2020, 1, 1), new(2020, 1, 2), false)));
        var html = await client.GetStringAsync(path);
        var post = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(html), ["Input.OperationId"] = "resume",
            ["Input.Version"] = tracking.Version.ToString(), ["Input.StateCode"] = "in_progress",
            ["Input.KeepCycle"] = "true", ["Input.Start"] = "2020-01-01"
        };
        Assert.Equal(HttpStatusCode.Redirect,
            (await client.PostAsync(path + "&handler=Save", new FormUrlEncodedContent(post))).StatusCode);
        await using var db = new AppDbContext(app.Paths);
        Assert.Equal(new DateOnly(2020, 1, 2), (await db.Cycles.AsNoTracking().SingleAsync()).CompletionOn);
        Assert.Empty(await db.Events.Where(e => e.Type == "date_corrected").ToArrayAsync());

        post["__RequestVerificationToken"] = Token(await client.GetStringAsync(path));
        post["Input.OperationId"] = "clear-date";
        post["Input.Version"] = (tracking.Version + 1).ToString();
        post["Input.StateCode"] = "completed";
        post["Input.Completion"] = "";
        Assert.Equal(HttpStatusCode.Redirect,
            (await client.PostAsync(path + "&handler=Save", new FormUrlEncodedContent(post))).StatusCode);
        Assert.Null((await db.Cycles.AsNoTracking().SingleAsync()).CompletionOn);
        var correction = await db.Events.SingleAsync(e => e.Type == "date_corrected");
        Assert.Equal("completion", correction.Field);
        Assert.Equal("2020-01-02", correction.OldValue);
        Assert.Null(correction.NewValue);
    }

    [Fact]
    public async Task NewCycleAndHistoricalDateCorrectionUseContextWithoutChangingOtherCycleData()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        _ = await client.GetStringAsync("/Works/Details?id=cidade-distante");
        var tracking = await new TrackingService(app.Paths).AddAsync(new("finished", "cidade-distante", "completed",
            new(new(2020, 1, 1), new(2020, 1, 2), false)));
        var cyclePath = "/Works/NewCycle?id=" + tracking.TrackingId;
        var cyclePost = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(await client.GetStringAsync(cyclePath)),
            ["Input.OperationId"] = "new-cycle", ["Input.Version"] = tracking.Version.ToString(),
            ["Input.StateCode"] = "in_progress", ["Input.Start"] = "2099-01-01", ["Input.Completion"] = "2099-02-01"
        };
        var warning = await client.PostAsync(cyclePath, new FormUrlEncodedContent(cyclePost));
        Assert.Equal(HttpStatusCode.OK, warning.StatusCode);
        var warningHtml = await warning.Content.ReadAsStringAsync();
        Assert.Contains("A data de início está no futuro.", WebUtility.HtmlDecode(warningHtml));
        Assert.DoesNotContain("A data de conclusão está no futuro.", WebUtility.HtmlDecode(warningHtml));
        cyclePost["Input.ConfirmDates"] = "true";
        cyclePost["Input.DateConfirmationKey"] = PatternValue(warningHtml, "Input.DateConfirmationKey");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(cyclePath, new FormUrlEncodedContent(cyclePost))).StatusCode);

        await using var db = new AppDbContext(app.Paths);
        var cycles = await db.Cycles.AsNoTracking().OrderBy(c => c.Ordinal).ToArrayAsync();
        Assert.Equal(2, cycles.Length);
        Assert.Null(cycles[1].CompletionOn);
        var historyPath = "/Works/History?id=" + tracking.TrackingId;
        var historyHtml = await client.GetStringAsync(historyPath);
        Assert.DoesNotContain("Manter as datas informadas", WebUtility.HtmlDecode(historyHtml));
        var historyPost = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(historyHtml), ["OperationId"] = "correct-cycle",
            ["Version"] = (tracking.Version + 1).ToString(), ["CycleId"] = cycles[0].Id,
            ["Field"] = "completion", ["Value"] = "2019-12-31"
        };
        var historyWarning = await client.PostAsync(historyPath, new FormUrlEncodedContent(historyPost));
        Assert.Equal(HttpStatusCode.OK, historyWarning.StatusCode);
        var historyWarningHtml = await historyWarning.Content.ReadAsStringAsync();
        Assert.Contains("A conclusão vem antes da data de início.", WebUtility.HtmlDecode(historyWarningHtml));
        historyPost["ConfirmDates"] = "true";
        historyPost["DateConfirmationKey"] = PatternValue(historyWarningHtml, "DateConfirmationKey");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(historyPath, new FormUrlEncodedContent(historyPost))).StatusCode);
        Assert.Equal(new DateOnly(2019, 12, 31), (await db.Cycles.AsNoTracking().SingleAsync(c => c.Ordinal == 1)).CompletionOn);
        Assert.Equal(new DateOnly(2099, 1, 1), (await db.Cycles.AsNoTracking().SingleAsync(c => c.Ordinal == 2)).StartOn);
        Assert.Equal("in_progress", (await db.Trackings.SingleAsync()).StateCode);
        Assert.Equal("2020-01-02", (await db.Events.SingleAsync(e => e.Type == "date_corrected")).OldValue);
    }

    private static string Token(string html) => PatternValue(html, "__RequestVerificationToken");

    private static string PatternValue(string html, string name)
    {
        var value = Regex.Match(html, $"<input[^>]*name=\"{Regex.Escape(name)}\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(value);
        return WebUtility.HtmlDecode(value);
    }

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
