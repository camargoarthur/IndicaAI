namespace IndicaAI.Web.Hosting;

public sealed record DataPaths(string Root)
{
    public string Database => Path.Combine(Root, "data", "indicaai.db");
    public string Assets => Path.Combine(Root, "assets");
    public string Backups => Path.Combine(Root, "backups");
    public static DataPaths ForRoot(string root) => new(Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)));
    public void EnsureDirectories()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Database)!);
        Directory.CreateDirectory(Assets);
        Directory.CreateDirectory(Backups);
    }
}
