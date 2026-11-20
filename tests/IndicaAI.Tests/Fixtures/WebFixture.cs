using IndicaAI.Web.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IndicaAI.Tests.Fixtures;
public sealed class WebFixture : WebApplicationFactory<global::Program>
{
    public DataPaths Paths { get; } = DataPaths.ForRoot(Path.Combine(Path.GetTempPath(), "IndicaAI-web-tests", Guid.NewGuid().ToString("N")));
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseSetting("IndicaAI:DemoCatalog", "true")
        .ConfigureServices(services => { services.RemoveAll<DataPaths>(); services.AddSingleton(Paths); });
}
