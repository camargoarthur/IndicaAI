using System.Data.Common;
using System.Text;
using System.Text.Json;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Archive;
using IndicaAI.Web.Features.Batch;
using IndicaAI.Web.Features.Imports;
using IndicaAI.Web.Features.Lists;
using IndicaAI.Web.Features.Settings;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Operations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IndicaAI.Tests.Integration;

public class RecoveryAndBatchTests
{
    [Fact]
    public async Task LastWriteFailureRollsBackEntirePersonalBatch()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var s = new TrackingService(f.Paths); var a = await s.AddAsync(new("a", "alem-do-vale", "planned", new(null, null, false))); var b = await s.AddAsync(new("b", "cidade-distante", "planned", new(null, null, false)));
        await Assert.ThrowsAsync<DbUpdateException>(() => new BatchService(f.Paths, [new FailTracking(b.TrackingId)]).ChangeStatesAsync("batch", [new(a.TrackingId, 1), new(b.TrackingId, 1)], new Dictionary<string, string> { { "jogo", "completed" }, { "livro", "completed" } }));
        await using var db = f.Open(); Assert.Equal(2, await db.Trackings.CountAsync(t => t.StateCode == "planned" && t.Version == 1)); Assert.Equal(2, await db.Events.CountAsync()); Assert.False(await db.Receipts.AnyAsync(r => r.OperationId == "batch"));
    }
    [Fact]
    public async Task SnapshotRestoresEveryBusinessTableAndAssetExactly()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var s = new TrackingService(f.Paths); var a = await s.AddAsync(new("a", "alem-do-vale", "completed", new(new(2020, 1, 2), new(2020, 2, 2), false), "nota")); var next = await s.StartCycleAsync(new(new("cycle", 1), a.TrackingId, "in_progress", new(null, null, false))); await using (var db = f.Open()) { var cycle = await db.Cycles.SingleAsync(c => c.Ordinal == 1); await s.CorrectDateAsync(new(new("correct", next.Version), a.TrackingId, cycle.Id, "start", new(2019, 12, 1), false)); }
        var list = await new ListService(f.Paths).SaveAsync("list", null, 0, "Favoritos"); await new ListService(f.Paths).SetMembershipAsync(new("member", 3), a.TrackingId, list.Id, true); await new ArchiveService(f.Paths).SetAsync(new("archive", 4), a.TrackingId, true);
        var tags = new TagService(f.Paths); var group = await tags.SaveGroupAsync("group", null, 0, "Gênero"); var preview = await new ImportPreviewService(f.Paths).CreateAsync(new MemoryStream(Encoding.UTF8.GetBytes($"tag_id;nome;grupo_id;midias\nsnap;Aventura;{group.Id};jogo\n")), "tags"); await new ImportApplyService(f.Paths).ApplyAsync(preview.BatchId, preview.PlanHash); await tags.AssociateAsync("assoc", "alem-do-vale", "snap"); await new PreferenceService(f.Paths).SetAsync(new("preferences", 1), "dark", "en");
        var before = await Snapshot(f.Paths.Database); var assetHashes = Directory.GetFiles(f.Paths.Assets, "*", SearchOption.AllDirectories).Order().Select(p => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(p)))).ToArray();
        var bundle = await new BackupService(f.Paths).CreateAsync("manual"); await s.SaveNoteAsync(new(new("after", 5), a.TrackingId, "depois")); await new RestoreService(f.Paths).RestoreAsync(bundle); Assert.Equal(before, await Snapshot(f.Paths.Database)); Assert.Equal(assetHashes, Directory.GetFiles(f.Paths.Assets, "*", SearchOption.AllDirectories).Order().Select(p => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(p)))).ToArray());
    }
    [Fact]
    public async Task StartupRejectsUnknownSchemaAndRecoversInterruptedDirectorySwap()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); await using (var db = f.Open()) await db.Database.ExecuteSqlRawAsync("INSERT INTO __EFMigrationsHistory VALUES ('20990101000000_Future','99.0.0')"); var before = await File.ReadAllBytesAsync(f.Paths.Database); await Assert.ThrowsAsync<DomainException>(() => f.InitializeAsync()); Assert.Equal(before, await File.ReadAllBytesAsync(f.Paths.Database));
        var other = new TestDatabase(); await other.InitializeAsync(); var original = await Snapshot(other.Paths.Database); var previous = Path.Combine(other.Paths.Root, "recovery-test"); Directory.CreateDirectory(previous); Directory.Move(Path.Combine(other.Paths.Root, "data"), Path.Combine(previous, "data")); var stage = Path.Combine(other.Paths.Root, "restore-stage-test"); Directory.CreateDirectory(stage); await File.WriteAllTextAsync(Path.Combine(other.Paths.Root, "restore-pending.json"), JsonSerializer.Serialize(new { Stage = "restore-stage-test", Previous = "recovery-test" })); await other.InitializeAsync(); Assert.Equal(original, await Snapshot(other.Paths.Database)); Assert.False(File.Exists(Path.Combine(other.Paths.Root, "restore-pending.json")));
    }
    static async Task<string> Snapshot(string file) { await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = file, Pooling = false }.ToString()); await connection.OpenAsync(); var tables = new List<string>(); await using var command = connection.CreateCommand(); command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%' ORDER BY name"; await using (var reader = await command.ExecuteReaderAsync()) while (await reader.ReadAsync()) tables.Add(reader.GetString(0)); var output = new List<string>(); foreach (var table in tables) { command.CommandText = "SELECT * FROM \"" + table.Replace("\"", "\"\"") + "\""; var values = new List<string>(); await using var reader = await command.ExecuteReaderAsync(); while (await reader.ReadAsync()) { var row = new object[reader.FieldCount]; reader.GetValues(row); values.Add(JsonSerializer.Serialize(row.Select(v => v is DBNull ? null : v).ToArray())); } output.Add(table + ":" + string.Join("\n", values.Order())); } return string.Join("\n", output); }
    sealed class FailTracking(string id) : DbCommandInterceptor { public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) { if (command.CommandText.StartsWith("UPDATE \"Trackings\"") && command.Parameters.Cast<DbParameter>().Any(p => p.Value?.ToString() == id)) throw new InvalidOperationException("injected final item failure"); return base.ReaderExecutingAsync(command, eventData, result, cancellationToken); } }
}
