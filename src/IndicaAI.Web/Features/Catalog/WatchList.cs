using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IndicaAI.Web.Domain;

namespace IndicaAI.Web.Features.Catalog;

public sealed record WatchListItem(int Line, string? MediaCode, bool IsAnime, string Title, string AlternateTitle, int Year, string Description);

public static class WatchList
{
    private static readonly Regex Entry = new(@"\A-\s+(?<title>.+?)\s*\((?<alternate>.+?)\s+-\s+(?<year>(?:18|19|20)\d{2})\):\s+(?<description>.+)\z", RegexOptions.CultureInvariant);

    public static IReadOnlyList<WatchListItem> Read(TextReader input)
    {
        var result = new List<WatchListItem>();
        string? section = null;
        var number = 0;
        while (input.ReadLine() is { } raw)
        {
            number++;
            if (number > 1000 || raw.Length > 4096) throw new DomainException("watch_list_invalid", number);
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (!line.StartsWith('-') && line.EndsWith(':'))
            {
                section = TitleKey(line) switch
                {
                    "FILMES" => "filme",
                    "SERIES" => "serie",
                    "ANIMES" => "anime",
                    _ => throw new DomainException("watch_list_invalid", number)
                };
                continue;
            }
            var match = Entry.Match(line);
            if (section is null || !match.Success) throw new DomainException("watch_list_invalid", number);
            var title = match.Groups["title"].Value.Trim();
            var alternate = match.Groups["alternate"].Value.Trim();
            if (title.Length is 0 or > 160 || alternate.Length is 0 or > 160 || result.Count == 250)
                throw new DomainException("watch_list_invalid", number);
            var year = int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture);
            var anime = section == "anime";
            result.Add(new(number, anime ? null : section, anime, title, alternate, year, match.Groups["description"].Value));
        }
        if (result.Count == 0) throw new DomainException("watch_list_invalid", number);
        return result;
    }

    internal static string TitleKey(string value)
    {
        var result = new StringBuilder(value.Length);
        var separated = false;
        foreach (var character in value.Normalize(NormalizationForm.FormD).EnumerateRunes())
        {
            if (Rune.GetUnicodeCategory(character) is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark) continue;
            if (!Rune.IsLetterOrDigit(character)) { separated = true; continue; }
            if (separated && result.Length > 0) result.Append(' ');
            result.Append(Rune.ToUpperInvariant(character));
            separated = false;
        }
        return result.ToString();
    }
}
