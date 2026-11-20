namespace IndicaAI.Web.Localization;

public record SupportedLocale(string Code, string NativeName, bool Selectable = true, string DatePattern = "dd MMM yyyy", string InstantPattern = "dd MMM yyyy HH:mm");
public static class LocaleRegistry
{
    // Add a resource pair/catalog and register here; domain IDs and data do not change.
    public static IReadOnlyList<SupportedLocale> Registered => [new("pt-BR", "Português (Brasil)"), new("en", "English", DatePattern: "MMM d, yyyy", InstantPattern: "MMM d, yyyy HH:mm"), new("fr", "Localidade de teste", false)];
    public static IReadOnlyList<SupportedLocale> GetSupported() => Registered.Where(l => l.Selectable).ToArray();
}
