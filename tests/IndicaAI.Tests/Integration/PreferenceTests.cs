using System.Globalization;
using System.Xml.Linq;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Settings;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Localization;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public class PreferenceTests
{
    [Fact]
    public async Task EnglishKeepsPersonalContentAndReopensWithPreferences()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); await using var db = f.Open(); var w = await db.Works.FirstAsync();
        await new TrackingService(f.Paths).AddAsync(new("p-add", w.Id, "in_progress", new(new(2026, 11, 8), null, true), "Minha nota"));
        await new StateLabelService(f.Paths).RenameAsync(new("label", 1), w.MediaCode, "in_progress", "Lendo aos poucos");
        var service = new PreferenceService(f.Paths); await service.SetAsync(new("p-en", 2), "dark", "en");
        var pref = await new PreferenceService(f.Paths).GetAsync(); Assert.Equal("dark", pref.Theme); Assert.Equal("en", pref.Locale);
        await using var read = f.Open(); Assert.Equal("Minha nota", (await read.Trackings.SingleAsync()).Note); Assert.Equal("Lendo aos poucos", (await read.StateLabels.SingleAsync()).Label); Assert.Equal(1, await read.Events.CountAsync()); Assert.Equal(new DateOnly(2026, 11, 8), (await read.Cycles.SingleAsync()).StartOn);
        Assert.Equal("Lendo", UiMessages.State("livro", "in_progress", "fr"));
        Assert.Equal("Bibliothèque (test)", UiMessages.Lookup("Library", "fr"));
        Assert.Contains(LocaleRegistry.Registered, l => l.Code == "fr" && !l.Selectable);
        Assert.DoesNotContain(LocaleRegistry.GetSupported(), l => l.Code == "fr");
        foreach (var n in new[] { 0, 1, 2 }) Assert.DoesNotContain("{", UiMessages.Lookup("SelectedCount", "en", n));
        Assert.Equal("en", LocaleRegistry.GetSupported().Single(l => l.Code == "en").Code);
    }
    [Fact]
    public void CatalogsHaveIdenticalKeysAndParameters()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/IndicaAI.Web/Resources"));
        Dictionary<string, string> Read(string locale) => XDocument.Load(Path.Combine(root, "UiText." + locale + ".resx")).Root!.Elements("data").ToDictionary(x => x.Attribute("name")!.Value, x => x.Element("value")!.Value);
        var pt = Read("pt-BR"); var en = Read("en"); Assert.Equal(pt.Keys.Order(), en.Keys.Order());
        foreach (var key in pt.Keys) { var parameters = System.Text.RegularExpressions.Regex.Matches(pt[key], @"\{\d+").Select(m => m.Value).Order(); Assert.Equal(parameters, System.Text.RegularExpressions.Regex.Matches(en[key], @"\{\d+").Select(m => m.Value).Order()); }
    }
}
