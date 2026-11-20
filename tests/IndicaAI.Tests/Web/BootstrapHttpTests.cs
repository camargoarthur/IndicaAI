using IndicaAI.Web.Hosting;
using IndicaAI.Web.Pages;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace IndicaAI.Tests.Web;

public sealed class BootstrapHttpTests
{
    [Fact]
    public async Task LibraryAndCatalogOpenWithoutLogin()
    {
        var root = DataPaths.ForRoot(Path.Combine(Path.GetTempPath(), "IndicaAI-tests", Guid.NewGuid().ToString("N")));
        await using var factory = new WebApplicationFactory<global::Program>().WithWebHostBuilder(b =>
            b.ConfigureServices(s => { s.RemoveAll<DataPaths>(); s.AddSingleton(root); }));
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var library = await client.GetAsync("/Library");
        var catalog = await client.GetAsync("/Catalog");
        Assert.Equal(HttpStatusCode.OK, library.StatusCode);
        Assert.Equal(HttpStatusCode.OK, catalog.StatusCode);
        Assert.Null(library.Headers.Location);
        Assert.DoesNotContain("type=\"password\"", await library.Content.ReadAsStringAsync());
        Assert.DoesNotContain("A cidade distante", await catalog.Content.ReadAsStringAsync());
    }
}

