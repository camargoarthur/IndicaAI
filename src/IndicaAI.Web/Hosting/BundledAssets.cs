using IndicaAI.Web.Domain;
namespace IndicaAI.Web.Hosting;

public static class BundledAssets
{
    public static void Ensure(DataPaths paths)
    {
        var source = Path.Combine(AppContext.BaseDirectory, "wwwroot", "covers"); if (!Directory.Exists(source)) { var parent = new DirectoryInfo(AppContext.BaseDirectory); while (parent != null) { var candidate = Path.Combine(parent.FullName, "src", "IndicaAI.Web", "wwwroot", "covers"); if (Directory.Exists(candidate)) { source = candidate; break; } parent = parent.Parent; } }
        if (!Directory.Exists(source)) throw new DomainException("backup_invalid"); var destination = Directory.CreateDirectory(Path.Combine(paths.Assets, "covers")).FullName; foreach (var file in Directory.EnumerateFiles(source, "*.png")) { var target = Path.Combine(destination, Path.GetFileName(file)); if (!File.Exists(target)) File.Copy(file, target, false); }
    }
}
