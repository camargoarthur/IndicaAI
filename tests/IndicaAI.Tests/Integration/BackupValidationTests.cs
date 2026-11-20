using System.Text.Json;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Operations;
using Microsoft.Data.Sqlite;
namespace IndicaAI.Tests.Integration;

public class BackupValidationTests
{
    [Fact]
    public async Task MissingAssetsAndFutureSchemaAreRejectedBeforeReplacingActive()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var before = await File.ReadAllBytesAsync(f.Paths.Database); var service = new BackupService(f.Paths); var restore = new RestoreService(f.Paths); var missing = await service.CreateAsync("manual"); File.Delete(Directory.GetFiles(Path.Combine(missing, "assets", "covers"))[0]); await Assert.ThrowsAsync<DomainException>(() => restore.RestoreAsync(missing)); Assert.Equal(before, await File.ReadAllBytesAsync(f.Paths.Database));
        var future = await service.CreateAsync("manual"); var file = Path.Combine(future, "data", "indicaai.db"); await using (var connection = new SqliteConnection("Data Source=" + file + ";Pooling=False")) { await connection.OpenAsync(); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO __EFMigrationsHistory VALUES ('20990101000000_Future','99.0.0')"; await command.ExecuteNonQueryAsync(); }
        var manifestPath = Path.Combine(future, "manifest.json"); var manifest = JsonSerializer.Deserialize<BackupManifest>(await File.ReadAllTextAsync(manifestPath))!; manifest = manifest with { Migrations = [.. manifest.Migrations, "20990101000000_Future"], Files = manifest.Files.Select(entry => entry.Path == "data/indicaai.db" ? entry with { Size = new FileInfo(file).Length, Sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(awaitBytes(file))) } : entry).ToArray() }; await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest)); await Assert.ThrowsAsync<DomainException>(() => restore.RestoreAsync(future)); Assert.Equal(before, await File.ReadAllBytesAsync(f.Paths.Database));
        static byte[] awaitBytes(string path) => File.ReadAllBytes(path);
    }
    [Fact]
    public async Task DailyRetentionKeepsSevenVerifiedAndAllManualBundles()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var service = new BackupService(f.Paths); var manual = await service.CreateAsync("manual"); for (var i = 1; i <= 8; i++) { var bundle = await service.CreateAsync("daily"); var file = Path.Combine(bundle, "manifest.json"); var manifest = JsonSerializer.Deserialize<BackupManifest>(await File.ReadAllTextAsync(file))!; await File.WriteAllTextAsync(file, JsonSerializer.Serialize(manifest with { CreatedAtUtc = DateTime.UtcNow.AddDays(-i).ToString("O") })); }
        await service.DailyAsync(); Assert.Equal(7, Directory.GetDirectories(f.Paths.Backups, "b-d-*").Length); Assert.True(Directory.Exists(manual)); var before = Directory.GetDirectories(f.Paths.Backups).Order().ToArray(); await service.DailyAsync(); Assert.Equal(before, Directory.GetDirectories(f.Paths.Backups).Order().ToArray());
    }
}
