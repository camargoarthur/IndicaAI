using System.Globalization;
using System.Text;
namespace IndicaAI.Web.Text;

public static class UnicodeNameKey
{
    private static readonly Lazy<Dictionary<int,string>> Mappings = new(() =>
    {
        using var stream = typeof(UnicodeNameKey).Assembly.GetManifestResourceStream("IndicaAI.Web.Text.CaseFolding-17.0.0.txt")!;
        using var reader = new StreamReader(stream);
        var map = new Dictionary<int,string>();
        while (reader.ReadLine() is { } line)
        {
            line = line.Split('#')[0]; var parts = line.Split(';', StringSplitOptions.TrimEntries);
            if (parts.Length < 3 || parts[1] is not ("C" or "F")) continue;
            map[Convert.ToInt32(parts[0],16)] = string.Concat(parts[2].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => char.ConvertFromUtf32(Convert.ToInt32(s,16))));
        }
        return map;
    });
    public static string Create(string value)
    {
        var result = new StringBuilder();
        foreach (var rune in value.Trim().Normalize(NormalizationForm.FormC).EnumerateRunes())
            result.Append(Mappings.Value.TryGetValue(rune.Value, out var folded) ? folded : rune.ToString());
        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
