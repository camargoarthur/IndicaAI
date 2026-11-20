using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tracking;
namespace IndicaAI.Tests.Web;

public class CustomLabelTests
{
    [Fact]
    public async Task CustomLabelsAppearAcrossTrackingForms()
    {
        await using var app = new WebFixture(); using var client = app.CreateClient(); await client.GetAsync("/Library"); var tracking = await new TrackingService(app.Paths).AddAsync(new("a", "cidade-distante", "in_progress", new(null, null, false))); await new StateLabelService(app.Paths).RenameAsync(new("label", 1), "livro", "in_progress", "Lendo devagar");
        foreach (var path in new[] { "/Library", "/Works/Details?id=cidade-distante", "/Works/History?id=" + tracking.TrackingId, "/Works/NewCycle?id=" + tracking.TrackingId, "/Batch/Review?ids=" + tracking.TrackingId }) Assert.Contains("Lendo devagar", await client.GetStringAsync(path));
    }
}
