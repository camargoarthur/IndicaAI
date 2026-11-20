using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IndicaAI.Web.Domain;
namespace IndicaAI.Web.Features.Imports;

public record ImportPreview(string BatchId, string PlanHash, long Revision, IReadOnlyList<ImportRow> Rows);
public record ImportResult(string BatchId, string Status, int Applied, int Rejected, int Unprocessed, int IgnoredRecords, IReadOnlyList<ImportRow> Rows);
internal record TagPlan(string Id, long Version, string Name, string GroupId, string[] Media);
internal record AssociationPlan(string WorkId, string TagId);
internal static class ImportHash
{
    public static string Of(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    public static string Plan(ImportBatch batch, IEnumerable<ImportRow> rows) => Of(JsonSerializer.Serialize(new { batch.Type, batch.ContractVersion, batch.FileHash, batch.BaselineRevision, batch.IgnoredRecords, Rows = rows.OrderBy(r => r.RecordNumber).Select(r => new { r.RecordNumber, r.PhysicalStartLine, r.Key, r.Action, r.Status, r.ReasonCode, r.Before, r.After, r.Preserved }) }));
}

