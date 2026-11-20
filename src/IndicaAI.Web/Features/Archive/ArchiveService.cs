using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
namespace IndicaAI.Web.Features.Archive;

public sealed class ArchiveService(DataPaths paths)
{
    private readonly CommandRunner runner = new(paths);
    public Task<TrackingResult> SetAsync(Mutation mutation, string trackingId, bool archived, CancellationToken ct = default) => runner.RunAsync(mutation.OperationId, "archive.set", new { mutation, trackingId, archived }, async (db, owner) =>
    {
        var t = await TrackingService.FindOwned(db, owner, trackingId, ct); TrackingService.CheckVersion(t, mutation.ExpectedVersion);
        if (archived != (t.ArchivedAtUtc is not null)) { t.ArchivedAtUtc = archived ? DateTime.UtcNow.ToString("O") : null; t.Version++; }
        return new TrackingResult(t.Id, t.Version, t.StateCode, (await TrackingService.CurrentCycle(db, t.Id, ct)).Ordinal);
    }, ct);
}
