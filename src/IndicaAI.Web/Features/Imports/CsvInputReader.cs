using System.Globalization;
using System.Text;
using CsvHelper;
using IndicaAI.Web.Domain;
namespace IndicaAI.Web.Features.Imports;

public record CsvRecord(int Number, int Line, Dictionary<string, string> Values);
public record CsvDocument(string FileHash, IReadOnlyList<CsvRecord> Records, int IgnoredRecords);
public static class CsvInputReader
{
    public const int MaxBytes = 5 * 1024 * 1024;
    public static async Task<CsvDocument> ReadAsync(Stream input, string type, CancellationToken ct = default)
    {
        if (type is not ("tags" or "associacoes")) throw new DomainException("invalid_csv");
        using var buffer = new MemoryStream(); var chunk = new byte[65536]; int read; while ((read = await input.ReadAsync(chunk, ct)) > 0) { if (buffer.Length + read > MaxBytes) throw new DomainException("csv_limit"); buffer.Write(chunk, 0, read); }
        var bytes = buffer.ToArray();
        string text; try { text = new UTF8Encoding(false, true).GetString(bytes).TrimStart('\ufeff'); } catch (DecoderFallbackException) { throw new DomainException("invalid_csv"); }
        if (string.IsNullOrWhiteSpace(text)) throw new DomainException("empty_file");
        var leading = System.Text.RegularExpressions.Regex.Match(text, @"\A(?:[ \t]*(?:\r\n|\n))+").Value;
        int ignored = System.Text.RegularExpressions.Regex.Matches(leading, @"\r\n|\n").Count; var lineOffset = ignored; text = text[leading.Length..];
        try
        {
            var config = new CsvReaderOptions { Separator = ';', HasHeader = true, SkipEmptyLines = false, TrackLineNumbers = true, AllowLineBreaksInQuotes = true, MalformedData = CsvMalformedDataHandling.Throw, MissingField = CsvMissingFieldHandling.Throw, FormatProvider = CultureInfo.InvariantCulture };
            using var csv = CsvReader.FromText(text, config); await csv.ReadHeaderAsync(ct); var headers = csv.Headers.ToArray();
            var allowed = type == "tags" ? new[] { "tag_id", "nome", "grupo_id", "midias" } : new[] { "obra_id", "tag_id" }; if (headers.Length == 0 || headers.Distinct().Count() != headers.Length || headers.Any(h => !allowed.Contains(h)) || !headers.Contains("tag_id") || (type == "associacoes" && !headers.Contains("obra_id"))) throw new DomainException("invalid_csv");
            var rows = new List<CsvRecord>();
            while (await csv.ReadAsync(ct))
            {
                if (csv.FieldCount == 1 && string.IsNullOrWhiteSpace(csv.GetString(0))) { ignored++; continue; }
                if (csv.FieldCount != headers.Length) throw new DomainException("invalid_csv"); var line = (int)csv.LineNumber + lineOffset; var values = headers.Select((h, i) => new { h, i }).ToDictionary(p => p.h, p => csv.GetString(p.i).Trim().Normalize()); if (values.Values.All(string.IsNullOrEmpty)) { ignored++; continue; }
                if (rows.Count >= 10000) throw new DomainException("csv_limit"); rows.Add(new(rows.Count + 1, line, values));
            }
            if (rows.Count == 0) throw new DomainException("empty_file"); return new(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)), rows, ignored);
        }
        catch (CsvFormatException) { throw new DomainException("invalid_csv"); }
    }
}
