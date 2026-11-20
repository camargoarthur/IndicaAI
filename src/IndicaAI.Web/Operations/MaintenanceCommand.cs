using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
namespace IndicaAI.Web.Operations;

public static class MaintenanceCommand
{
    public static async Task<int> RunAsync(string[] args, CancellationToken ct = default)
    {
        try
        {
            string? Value(string key) { var index = Array.IndexOf(args, key); return index >= 0 && index + 1 < args.Length ? args[index + 1] : null; }
            var root = Value("--data-root") ?? Environment.GetEnvironmentVariable("INDICAAI_DATA_ROOT") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IndicaAI"); var paths = DataPaths.ForRoot(root); var action = Value("--maintenance");
            if (action == "backup") { using var instance = LocalInstance.Acquire(paths); Console.WriteLine(await new BackupService(paths).CreateAsync("manual", ct)); }
            else if (action == "validate") { await new RestoreService(paths).ValidateAsync(Value("--bundle") ?? throw new DomainException("backup_invalid"), ct); Console.WriteLine("OK"); }
            else if (action == "restore") { await new RestoreService(paths).RestoreAsync(Value("--bundle") ?? throw new DomainException("backup_invalid"), ct); Console.WriteLine("OK"); }
            else throw new DomainException("backup_invalid"); return 0;
        }
        catch (DomainException e) { Console.Error.WriteLine(e.Code); return 2; }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or Microsoft.Data.Sqlite.SqliteException) { Console.Error.WriteLine("operation_failed: " + e.GetType().Name); return 3; }
    }
}
