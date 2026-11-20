using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
namespace IndicaAI.Tests.Fixtures;
public sealed class TestDatabase
{
    public DataPaths Paths { get; } = DataPaths.ForRoot(Path.Combine(Path.GetTempPath(), "IndicaAI-tests", Guid.NewGuid().ToString("N")));
    public async Task InitializeAsync() => await new DatabaseBootstrapper(Paths).InitializeAsync();
    public AppDbContext Open() => new(Paths);
}
