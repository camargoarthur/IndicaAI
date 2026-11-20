using System.Net;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Localization;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Tests.Web;

public class QuickStateTests
{
    private sealed record Option(string Url, string Label, bool Active);

    private static Option[] Menu(string html)
    {
        var nav = Regex.Match(html, """<nav\b[^>]*class="[^"]*\bquick-states\b[^"]*"[^>]*>(.*?)</nav>""", RegexOptions.Singleline);
        return Regex.Matches(nav.Groups[1].Value, """<a\b([^>]*)>(.*?)</a>""", RegexOptions.Singleline)
            .Select(link => new Option(
                WebUtility.HtmlDecode(Regex.Match(link.Groups[1].Value, "href=\"([^\"]+)\"").Groups[1].Value),
                WebUtility.HtmlDecode(link.Groups[2].Value).Trim(),
                link.Groups[1].Value.Contains("aria-current=\"true\""))).ToArray();
    }

    [Fact]
    public async Task MediaOptionsFollowProductOrderLocaleAndPersonalLabels()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient();
        foreach (var media in MediaCodes.All)
        {
            var menu = Menu(await client.GetStringAsync("/Library?Media=" + media));
            Assert.Equal("Todos", Assert.Single(menu.Where(option => option.Active)).Label);
            Assert.Equal(MediaCodes.States.Select(state => UiMessages.State(media, state, "pt-BR")), menu.Skip(1).Select(option => option.Label));
        }
        Assert.Empty(Menu(await client.GetStringAsync("/Library")));
        Assert.Empty(Menu(await client.GetStringAsync("/Catalog?Media=filme")));
        Assert.Empty(Menu(await client.GetStringAsync("/Library?Context=list&Media=filme")));

        await using var db = new AppDbContext(app.Paths);
        var preferences = await db.Preferences.SingleAsync();
        preferences.Locale = "en";
        await db.SaveChangesAsync();
        const string label = "On my very long personal watch list";
        await new StateLabelService(app.Paths).RenameAsync(new("rename-quick", preferences.Version), "filme", "planned", label);
        var movieMenu = Menu(await client.GetStringAsync("/Library?Media=filme&QuickState=planned"));
        Assert.Equal("All", movieMenu[0].Label);
        Assert.Equal(label, Assert.Single(movieMenu.Where(option => option.Active)).Label);
        Assert.Contains(movieMenu, option => option.Label == UiMessages.State("filme", "completed", "en"));
        var bookMenu = Menu(await client.GetStringAsync("/Library?Media=filme&QuickState=planned&NewMedia=livro"));
        Assert.Equal("All", Assert.Single(bookMenu.Where(option => option.Active)).Label);
    }

    [Fact]
    public async Task LinksApplyStateIndependentlyOfAdvancedCriteriaAndRecoverFromEmptyResults()
    {
        await using var app = new WebFixture();
        using var client = app.CreateClient();
        await client.GetStringAsync("/Library");
        await new TrackingService(app.Paths).AddAsync(new("seed-quick", "mare-de-estrelas", "planned", new(null, null, false)));
        var start = "/Library?Media=filme&Search=estrelas&Mode=any&Include=state:filme/planned&Include=anime:true&Exclude=state:filme/paused";
        var menu = Menu(await client.GetStringAsync(start + "&PageNumber=2"));
        var target = menu.Single(option => option.Label == UiMessages.State("filme", "completed", "pt-BR"));
        var empty = WebUtility.HtmlDecode(await client.GetStringAsync(target.Url));
        Assert.DoesNotContain("class=\"work-card\"", empty);
        Assert.Contains(UiMessages.Lookup("WorkCount", "pt-BR", 0), empty);
        var filtered = Menu(empty);
        Assert.Equal(target.Label, Assert.Single(filtered.Where(option => option.Active)).Label);
        var allUrl = new Uri(client.BaseAddress!, filtered[0].Url);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(allUrl.Query);
        Assert.Equal("estrelas", query["Search"].ToString());
        Assert.Equal("any", query["Mode"].ToString());
        Assert.Equal(2, query["Include"].Count);
        Assert.Equal("state:filme/paused", query["Exclude"].ToString());
        var all = WebUtility.HtmlDecode(await client.GetStringAsync(allUrl));
        Assert.Contains("Maré de estrelas", all);
        Assert.Contains(UiMessages.Lookup("WorkCount", "pt-BR", 1), all);
        Assert.DoesNotContain("class=\"work-card\"", await client.GetStringAsync(start.Replace("Mode=any", "Mode=all") + "&QuickState=planned"));
        Assert.DoesNotContain("class=\"work-card\"", await client.GetStringAsync(start.Replace("Search=estrelas", "Search=ausente") + "&QuickState=planned"));
        Assert.DoesNotContain("class=\"work-card\"", await client.GetStringAsync(start.Replace("Exclude=state:filme/paused", "Exclude=state:filme/planned") + "&QuickState=planned"));
        await using var db = new AppDbContext(app.Paths);
        Assert.Equal("planned", (await db.Trackings.SingleAsync()).StateCode);
        Assert.Equal(1, await db.Events.CountAsync());
    }
}
