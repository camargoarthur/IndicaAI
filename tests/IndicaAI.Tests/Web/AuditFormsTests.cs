using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Web;

public sealed class AuditFormsTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HistoryPrefillsOnlyTheInformedDateForTheSelectedCycleAndField()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient();
        _ = await client.GetStringAsync("/Library", Ct);
        var service = new TrackingService(app.Paths);
        var tracking = await service.AddAsync(new("add-history", "cidade-distante", "completed",
            new(new(2020, 1, 1), new(2020, 1, 2), false)), Ct);
        await service.StartCycleAsync(new(new("next-history", tracking.Version), tracking.TrackingId, "in_progress",
            new(new(2021, 2, 3), null, false)), Ct);
        await using var db = new AppDbContext(app.Paths);
        var cycles = await db.Cycles.OrderBy(c => c.Ordinal).ToArrayAsync(Ct);
        var path = "/Works/History?id=" + tracking.TrackingId;

        Assert.Equal("2021-02-03", InputValue(await client.GetStringAsync(path, Ct), "Value"));
        Assert.Equal("2020-01-01", InputValue(await client.GetStringAsync(path + "&cycleId=" + cycles[0].Id + "&field=start", Ct), "Value"));
        Assert.Equal("2020-01-02", InputValue(await client.GetStringAsync(path + "&cycleId=" + cycles[0].Id + "&field=completion", Ct), "Value"));
        Assert.Equal("", InputValue(await client.GetStringAsync(path + "&cycleId=" + cycles[1].Id + "&field=completion", Ct), "Value"));
    }

    [Fact]
    public async Task HistoryRequiresExplicitClearingAndRecordsTheOriginalDateOnce()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        _ = await client.GetStringAsync("/Library", Ct);
        var tracking = await new TrackingService(app.Paths).AddAsync(new("add-clear", "cidade-distante", "completed",
            new(new(2020, 1, 1), new(2020, 1, 2), false)), Ct);
        await using var db = new AppDbContext(app.Paths);
        var cycle = await db.Cycles.AsNoTracking().SingleAsync(Ct);
        var path = "/Works/History?id=" + tracking.TrackingId;
        var html = await client.GetStringAsync(path, Ct);
        var post = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = InputValue(html, "__RequestVerificationToken"),
            ["OperationId"] = "clear-history",
            ["Version"] = tracking.Version.ToString(CultureInfo.InvariantCulture),
            ["CycleId"] = cycle.Id,
            ["Field"] = "start",
            ["Value"] = ""
        };

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(path, new FormUrlEncodedContent(post), Ct)).StatusCode);
        Assert.Equal(new DateOnly(2020, 1, 1), (await db.Cycles.AsNoTracking().SingleAsync(Ct)).StartOn);
        Assert.Empty(await db.Events.Where(e => e.Type == "date_corrected").ToArrayAsync(Ct));

        post["ClearDate"] = "true";
        post["Value"] = "2020-01-01";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(path, new FormUrlEncodedContent(post), Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(path, new FormUrlEncodedContent(post), Ct)).StatusCode);
        var updated = await db.Cycles.AsNoTracking().SingleAsync(Ct);
        Assert.Null(updated.StartOn);
        Assert.Equal(new DateOnly(2020, 1, 2), updated.CompletionOn);
        var correction = await db.Events.SingleAsync(e => e.Type == "date_corrected", Ct);
        Assert.Equal("2020-01-01", correction.OldValue);
        Assert.Null(correction.NewValue);
        Assert.NotNull(correction.CorrectedEventId);
    }

    [Fact]
    public async Task HistoryDoesNotSaveAValueLoadedForADifferentSelection()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        _ = await client.GetStringAsync("/Library", Ct);
        var tracking = await new TrackingService(app.Paths).AddAsync(new("add-context", "cidade-distante", "completed",
            new(new(2020, 1, 1), new(2020, 1, 2), false)), Ct);
        await using var db = new AppDbContext(app.Paths);
        var cycle = await db.Cycles.AsNoTracking().SingleAsync(Ct);
        var path = "/Works/History?id=" + tracking.TrackingId;
        var html = await client.GetStringAsync(path, Ct);
        var post = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = InputValue(html, "__RequestVerificationToken"),
            ["OperationId"] = "stale-field",
            ["Version"] = tracking.Version.ToString(CultureInfo.InvariantCulture),
            ["CycleId"] = cycle.Id,
            ["Field"] = "completion",
            ["LoadedDateContext"] = cycle.Id + "|start",
            ["Value"] = "2020-01-01"
        };

        var result = await client.PostAsync(path, new FormUrlEncodedContent(post), Ct);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("2020-01-02", InputValue(await result.Content.ReadAsStringAsync(Ct), "Value"));
        Assert.Empty(await db.Events.Where(e => e.Type == "date_corrected").ToArrayAsync(Ct));
    }

    [Fact]
    public async Task PreferencesSaveBothLabelsTogetherAndReplayOneMutation()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var html = await client.GetStringAsync("/Settings", Ct);
        var form = MediaLabelForm(html, "livro");
        Assert.Contains("handler=Labels", form);
        var post = LabelPost(html, "save-labels", 1, "Na minha fila", "Lendo aos poucos");
        foreach (var code in new[] { "paused", "completed", "dropped" })
            post[$"changes[{code}]"] = InputValue(form, $"changes[{code}]");

        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Settings?handler=Labels", new FormUrlEncodedContent(post), Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Settings?handler=Labels", new FormUrlEncodedContent(post.Reverse()), Ct)).StatusCode);
        await using var db = new AppDbContext(app.Paths);
        Assert.Equal(2, (await db.Preferences.SingleAsync(Ct)).Version);
        var labels = await db.StateLabels.ToDictionaryAsync(l => l.StateCode, l => l.Label, Ct);
        Assert.Equal("Na minha fila", labels["planned"]);
        Assert.Equal("Lendo aos poucos", labels["in_progress"]);
        Assert.Equal(2, labels.Count);
    }

    [Fact]
    public async Task PreferencesRejectDuplicateLabelsWithoutSavingPartAndKeepAllEnteredValues()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var post = LabelPost(await client.GetStringAsync("/Settings", Ct), "duplicate-labels", 1, "A minha fila", "A MINHA FILA");

        var result = await client.PostAsync("/Settings?handler=Labels", new FormUrlEncodedContent(post), Ct);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var html = await result.Content.ReadAsStringAsync(Ct);
        Assert.Contains("role=\"alert\"", html);
        Assert.Contains("A minha fila", html);
        Assert.Contains("A MINHA FILA", html);
        await using var db = new AppDbContext(app.Paths);
        Assert.Empty(await db.StateLabels.ToArrayAsync(Ct));
        Assert.Equal(1, (await db.Preferences.SingleAsync(Ct)).Version);
    }

    [Fact]
    public async Task PreferencesCanSwapExistingLabelsThenRestoreOneDefault()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var html = await client.GetStringAsync("/Settings", Ct);
        var service = new StateLabelService(app.Paths);
        await service.RenameAsync(new("label-planned", 1), "livro", "planned", "Minha fila", Ct);
        await service.RenameAsync(new("label-progress", 2), "livro", "in_progress", "Em mãos", Ct);
        var swap = LabelPost(html, "swap-labels", 3, "Em mãos", "Minha fila");

        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Settings?handler=Labels", new FormUrlEncodedContent(swap), Ct)).StatusCode);
        await using var db = new AppDbContext(app.Paths);
        var swapped = await db.StateLabels.ToDictionaryAsync(l => l.StateCode, l => l.Label, Ct);
        Assert.Equal("Em mãos", swapped["planned"]);
        Assert.Equal("Minha fila", swapped["in_progress"]);

        var reset = LabelPost(html, "reset-label", 4, "Em mãos", "Leitura atual");
        reset["resetCodes"] = "planned";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Settings?handler=Labels", new FormUrlEncodedContent(reset), Ct)).StatusCode);
        var remaining = await db.StateLabels.AsNoTracking().SingleAsync(Ct);
        Assert.Equal("in_progress", remaining.StateCode);
        Assert.Equal("Leitura atual", remaining.Label);
        Assert.Equal(5, (await db.Preferences.SingleAsync(Ct)).Version);
    }

    [Fact]
    public async Task PreferencesKeepStaleEditsForReviewWithoutOverwritingConcurrentUpdates()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var html = await client.GetStringAsync("/Settings", Ct);
        var post = LabelPost(html, "stale-labels", 1, "Minha alteração", "Meu texto");
        await new StateLabelService(app.Paths).RenameAsync(new("other-tab", 1), "livro", "in_progress", "Em outra aba", Ct);

        var result = await client.PostAsync("/Settings?handler=Labels", new FormUrlEncodedContent(post), Ct);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var form = MediaLabelForm(await result.Content.ReadAsStringAsync(Ct), "livro");
        Assert.Equal("Minha alteração", InputValue(form, "changes[planned]"));
        Assert.Equal("Meu texto", InputValue(form, "changes[in_progress]"));
        Assert.Equal("1", InputValue(form, "version"));

        await using var db = new AppDbContext(app.Paths);
        Assert.Equal("Em outra aba", (await db.StateLabels.SingleAsync(Ct)).Label);
        Assert.Equal(2, (await db.Preferences.SingleAsync(Ct)).Version);
    }

    private static Dictionary<string, string> LabelPost(string html, string operation, long version, string planned, string inProgress) => new()
    {
        ["__RequestVerificationToken"] = InputValue(html, "__RequestVerificationToken"),
        ["operationId"] = operation,
        ["version"] = version.ToString(CultureInfo.InvariantCulture),
        ["media"] = "livro",
        ["changes[planned]"] = planned,
        ["changes[in_progress]"] = inProgress
    };

    private static string InputValue(string html, string name)
    {
        var input = Regex.Match(html, $"<input\\b[^>]*\\bname=\"{Regex.Escape(name)}\"[^>]*>");
        Assert.True(input.Success, $"The response is missing the {name} input.");
        return WebUtility.HtmlDecode(Regex.Match(input.Value, "\\bvalue=\"([^\"]*)\"").Groups[1].Value);
    }

    private static string MediaLabelForm(string html, string media)
    {
        var form = Regex.Match(html, $"<section[^>]*id=\"labels-{media}\"[^>]*>.*?(<form\\b.*?</form>)", RegexOptions.Singleline);
        Assert.True(form.Success, $"The response is missing the form for {media} labels.");
        return form.Groups[1].Value;
    }
}
