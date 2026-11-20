using System.Text.Json;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
namespace IndicaAI.Web.Operations;

public sealed class RestoreService(DataPaths paths)
{
    public record SwapJournal(string Stage, string Previous);
    public static async Task RecoverPendingAsync(DataPaths paths, CancellationToken ct = default)
    {
        var marker = Path.Combine(paths.Root, "restore-pending.json"); if (!File.Exists(marker)) return;
        var journal = JsonSerializer.Deserialize<SwapJournal>(await File.ReadAllTextAsync(marker, ct)) ?? throw new DomainException("backup_invalid");
        if (!System.Text.RegularExpressions.Regex.IsMatch(journal.Stage, @"\Arestore-stage-[A-Za-z0-9-]+\z") || !System.Text.RegularExpressions.Regex.IsMatch(journal.Previous, @"\Arecovery-[A-Za-z0-9-]+\z")) throw new DomainException("backup_invalid");
        var previous = BackupService.SafePath(paths.Root, journal.Previous); var stage = BackupService.SafePath(paths.Root, journal.Stage); Directory.CreateDirectory(stage);
        foreach (var part in new[] { "data", "assets" }) { var old = Path.Combine(previous, part); var active = Path.Combine(paths.Root, part); if (Directory.Exists(old)) { BackupService.RejectLink(old); if (Directory.Exists(active)) { var retained = Path.Combine(stage, "retained-" + part + "-" + Guid.NewGuid().ToString("N")[..8]); Directory.Move(active, retained); } Directory.Move(old, active); } }
        if (!File.Exists(paths.Database)) throw new DomainException("backup_invalid"); File.Delete(marker);
    }
    public async Task<BackupManifest> ValidateAsync(string bundle, CancellationToken ct = default)
    {
        try
        {
            BackupService.RejectLink(bundle); var manifest = JsonSerializer.Deserialize<BackupManifest>(await File.ReadAllTextAsync(Path.Combine(bundle, "manifest.json"), ct)) ?? throw new DomainException("backup_invalid");
            if (manifest.Format != 1 || manifest.Files is null || manifest.Migrations is null || manifest.Files.Length == 0 || manifest.Files.Select(f => f.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() != manifest.Files.Length || !manifest.Files.Any(f => f.Path == "data/indicaai.db")) throw new DomainException("backup_invalid");
            foreach (var file in manifest.Files) { var path = BackupService.SafePath(bundle, file.Path); if (!(file.Path == "data/indicaai.db" || file.Path.StartsWith("assets/", StringComparison.Ordinal))) throw new DomainException("backup_invalid"); BackupService.RejectLink(path); var info = new FileInfo(path); if (info.Length != file.Size || BackupService.HashFile(path) != file.Sha256) throw new DomainException("backup_invalid"); }
            var actual = BackupService.SafeFiles(bundle).Select(p => Path.GetRelativePath(bundle, p).Replace('\\', '/')).Where(p => p != "manifest.json").Order().ToArray(); if (!actual.SequenceEqual(manifest.Files.Select(f => f.Path).Order())) throw new DomainException("backup_invalid");
            var migrations = await BackupService.ValidateDatabase(DataPaths.ForRoot(bundle), ct); if (!migrations.SequenceEqual(manifest.Migrations)) throw new DomainException("backup_invalid"); return manifest;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or Microsoft.Data.Sqlite.SqliteException) { throw new DomainException("backup_invalid"); }
    }
    public async Task RestoreAsync(string bundle, CancellationToken ct = default)
    {
        using var instance = LocalInstance.Acquire(paths); var gate = DatabaseGate.For(paths); await gate.WaitAsync(ct); try
        {
            await ValidateAsync(bundle, ct); paths.EnsureDirectories();
            if(File.Exists(paths.Database))
            {
                // A damaged active set must remain recoverable. The complete original directories
                // are retained by the journaled swap below even when they cannot form a valid bundle.
                try { await new BackupService(paths).CreateCoreAsync("pre-restore", ct); }
                catch(DomainException e) when(e.Code=="backup_invalid") { }
                catch(Microsoft.Data.Sqlite.SqliteException) { }
                catch(IOException) { }
            }
            var stage = Path.Combine(paths.Root, "restore-stage-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(stage); BackupService.CopyTree(Path.Combine(bundle, "data"), Path.Combine(stage, "data")); BackupService.CopyTree(Path.Combine(bundle, "assets"), Path.Combine(stage, "assets")); await BackupService.ValidateDatabase(DataPaths.ForRoot(stage), ct);
            var previous = Path.Combine(paths.Root, "recovery-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(previous); var marker = Path.Combine(paths.Root, "restore-pending.json"); await File.WriteAllTextAsync(marker, JsonSerializer.Serialize(new SwapJournal(Path.GetFileName(stage), Path.GetFileName(previous))), ct);
            try { if (Directory.Exists(Path.Combine(paths.Root, "data"))) Directory.Move(Path.Combine(paths.Root, "data"), Path.Combine(previous, "data")); if (Directory.Exists(paths.Assets)) Directory.Move(paths.Assets, Path.Combine(previous, "assets")); Directory.Move(Path.Combine(stage, "data"), Path.Combine(paths.Root, "data")); Directory.Move(Path.Combine(stage, "assets"), paths.Assets); File.Delete(marker); }
            catch { await RecoverPendingAsync(paths, CancellationToken.None); throw; }
        }
        finally { gate.Release(); }
    }
}
