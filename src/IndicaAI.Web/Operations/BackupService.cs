using System.Security.Cryptography;
using System.Text.Json;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Operations;

public record BackupFile(string Path, long Size, string Sha256);
public record BackupManifest(int Format, string CreatedAtUtc, string Reason, string[] Migrations, BackupFile[] Files);
public sealed class BackupService(DataPaths paths)
{
    public async Task<string> CreateAsync(string reason, CancellationToken ct = default) { var gate = DatabaseGate.For(paths); await gate.WaitAsync(ct); try { return await CreateCoreAsync(reason, ct); } finally { gate.Release(); } }
    internal async Task<string> CreateCoreAsync(string reason, CancellationToken ct)
    {
        if (reason is not ("daily" or "pre-migration" or "manual" or "pre-restore" or "pre-demo-cleanup")) throw new DomainException("backup_invalid");
        var code = reason switch { "daily" => "d", "pre-migration" => "u", "pre-restore" => "r", "pre-demo-cleanup" => "c", _ => "m" };
        paths.EnsureDirectories(); var name = "b-" + code + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..8]; var pending = Path.Combine(paths.Backups, ".p-" + name); Directory.CreateDirectory(Path.Combine(pending, "data"));
        await using (var source = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = paths.Database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString())) { await source.OpenAsync(ct); await using var target = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.Combine(pending, "data", "indicaai.db"), Pooling = false }.ToString()); await target.OpenAsync(ct); source.BackupDatabase(target); await using var mode = target.CreateCommand(); mode.CommandText = "PRAGMA journal_mode=DELETE"; await mode.ExecuteNonQueryAsync(ct); }
        if (Directory.Exists(paths.Assets)) CopyTree(paths.Assets, Path.Combine(pending, "assets")); else Directory.CreateDirectory(Path.Combine(pending, "assets"));
        var snapshot = DataPaths.ForRoot(pending); var migrations = await ValidateDatabase(snapshot, ct);
        var files = SafeFiles(pending).Select(file => new BackupFile(Path.GetRelativePath(pending, file).Replace('\\', '/'), new FileInfo(file).Length, HashFile(file))).OrderBy(f => f.Path).ToArray();
        var manifest = new BackupManifest(1, DateTime.UtcNow.ToString("O"), reason, migrations, files); await File.WriteAllTextAsync(Path.Combine(pending, "manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }), ct);
        await new RestoreService(paths).ValidateAsync(pending, ct); var complete = Path.Combine(paths.Backups, name); Directory.Move(pending, complete); return complete;
    }
    public async Task DailyAsync(CancellationToken ct = default)
    {
        if (!File.Exists(paths.Database)) return; paths.EnsureDirectories(); var verified = new List<(string Path, BackupManifest Manifest)>(); foreach (var dir in Directory.EnumerateDirectories(paths.Backups, "b-d-*")) { try { var manifest = await new RestoreService(paths).ValidateAsync(dir, ct); verified.Add((dir, manifest)); } catch (DomainException) {/* Retain a damaged bundle for diagnosis; never count or erase it. */} }
        if (!verified.Any(v => v.Manifest.CreatedAtUtc.StartsWith(DateTime.UtcNow.ToString("yyyy-MM-dd"), StringComparison.Ordinal))) { var bundle = await CreateAsync("daily", ct); verified.Add((bundle, await new RestoreService(paths).ValidateAsync(bundle, ct))); }
        foreach (var old in verified.OrderByDescending(v => v.Manifest.CreatedAtUtc).Skip(7)) { var resolved = Path.GetFullPath(old.Path); if (!resolved.StartsWith(Path.GetFullPath(paths.Backups) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new DomainException("backup_invalid"); Directory.Delete(resolved, true); }
    }
    internal static void CopyTree(string source, string destination) { RejectLink(source); Directory.CreateDirectory(destination); foreach (var file in Directory.EnumerateFiles(source)) { RejectLink(file); File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false); } foreach (var dir in Directory.EnumerateDirectories(source)) CopyTree(dir, Path.Combine(destination, Path.GetFileName(dir))); }
    internal static void RejectLink(string path) { if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new DomainException("backup_invalid"); }
    internal static IEnumerable<string> SafeFiles(string directory) { RejectLink(directory); foreach (var file in Directory.EnumerateFiles(directory)) { RejectLink(file); yield return file; } foreach (var child in Directory.EnumerateDirectories(directory)) foreach (var file in SafeFiles(child)) yield return file; }
    internal static string HashFile(string file) { using var stream = File.OpenRead(file); return Convert.ToHexString(SHA256.HashData(stream)); }
    internal static async Task<string[]> ValidateDatabase(DataPaths snapshot, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = snapshot.Database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()); await connection.OpenAsync(ct);
        await using var check = connection.CreateCommand(); check.CommandText = "PRAGMA integrity_check"; if ((await check.ExecuteScalarAsync(ct))?.ToString() != "ok") throw new DomainException("backup_invalid"); check.CommandText = "PRAGMA foreign_key_check"; await using (var reader = await check.ExecuteReaderAsync(ct)) { if (await reader.ReadAsync(ct)) throw new DomainException("backup_invalid"); }
        // Use read-only SQL for validation, never EF migrations/seed on a restore candidate.
        check.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId"; var ids = new List<string>(); await using (var reader = await check.ExecuteReaderAsync(ct)) { while (await reader.ReadAsync(ct)) ids.Add(reader.GetString(0)); }
        await using var db = new AppDbContext(snapshot); var known = db.Database.GetMigrations().ToArray(); if (ids.Count == 0 || ids.Any(id => !known.Contains(id))) throw new DomainException("backup_invalid");
        check.CommandText = "SELECT CoverAsset FROM Works WHERE CoverAsset IS NOT NULL"; await using (var reader = await check.ExecuteReaderAsync(ct)) { while (await reader.ReadAsync(ct)) { var relative = "assets/covers/" + reader.GetString(0); var file = SafePath(snapshot.Root, relative); if (!File.Exists(file)) throw new DomainException("backup_invalid"); } }
        return ids.ToArray();
    }
    internal static string SafePath(string root, string relative) { if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains(':') || relative.Split('/', '\\').Any(p => p is ".." or "." or "")) throw new DomainException("backup_invalid"); var resolved = Path.GetFullPath(Path.Combine(root, relative)); if (!resolved.StartsWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new DomainException("backup_invalid"); return resolved; }
}

