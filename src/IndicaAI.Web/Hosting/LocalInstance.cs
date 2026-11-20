using IndicaAI.Web.Domain;
namespace IndicaAI.Web.Hosting;

public sealed class LocalInstance : IDisposable
{
    private readonly FileStream stream;
    private LocalInstance(FileStream stream) => this.stream = stream;
    public static LocalInstance Acquire(DataPaths paths) { Directory.CreateDirectory(paths.Root); try { return new(new FileStream(Path.Combine(paths.Root, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)); } catch (IOException) { throw new DomainException("instance_running"); } }
    public void Dispose() => stream.Dispose();
}
