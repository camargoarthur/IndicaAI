using System.Globalization;
using System.Resources;
using SmartFormat;

[assembly: NeutralResourcesLanguage("pt-BR", UltimateResourceFallbackLocation.Satellite)]
namespace IndicaAI.Web.Localization;

public sealed class UiMessages(IHttpContextAccessor? accessor = null)
{
    public string CurrentState(string media, string code) => (accessor?.HttpContext?.Items["StateLabels"] as IReadOnlyDictionary<string, string>)?.GetValueOrDefault(media + ":" + code) ?? State(media, code, Locale);
    private static readonly ResourceManager Resources = new("IndicaAI.Web.Resources.UiText", typeof(UiMessages).Assembly);
    public string Locale => LocaleRegistry.GetSupported().FirstOrDefault(l => l.Code == CultureInfo.CurrentUICulture.Name)?.Code ?? "pt-BR";
    public string this[string key] => Lookup(key, Locale);
    public string Get(string key, params object[] args) => Lookup(key, Locale, args);
    public static string Lookup(string key, string locale, params object[] args)
    {
        var culture = CultureInfo.GetCultureInfo(locale);
        var text = Resources.GetString(key, culture) ?? Resources.GetString(key, CultureInfo.GetCultureInfo("pt-BR")) ?? Resources.GetString("LoadError", CultureInfo.GetCultureInfo("pt-BR")) ?? "Mensagem indisponível";
        return args.Length == 0 ? text : Smart.Format(culture, text, args);
    }
    public static string State(string media, string code, string locale) => Lookup($"state.{media}.{code}", locale);
    public string Date(DateOnly? date) => date is null ? this["NoDate"] : date.Value.ToString(LocaleRegistry.Registered.Single(l => l.Code == Locale).DatePattern, CultureInfo.GetCultureInfo(Locale));
    public string Instant(string value) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture).ToLocalTime().ToString(LocaleRegistry.Registered.Single(l => l.Code == Locale).InstantPattern, CultureInfo.GetCultureInfo(Locale));
}
