using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Localization;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Settings;

public record PreferenceResult(string Theme, string Locale, long Version);
public sealed class PreferenceService(DataPaths paths)
{
    public async Task<PreferenceResult> GetAsync(CancellationToken ct = default) { await using var db = new AppDbContext(paths); var p = await db.Preferences.SingleAsync(ct); return new(p.Theme, p.Locale, p.Version); }
    public Task<PreferenceResult> SetAsync(Mutation mutation, string theme, string locale, CancellationToken ct = default) => new CommandRunner(paths).RunAsync(mutation.OperationId, "preferences.set", new { mutation, theme, locale }, async (db, owner) =>
    {
        if (theme is not ("light" or "dark") || !LocaleRegistry.GetSupported().Any(l => l.Code == locale)) throw new DomainException("preference_invalid");
        var p = await db.Preferences.SingleAsync(p => p.IdentityId == owner, ct); if (p.Version != mutation.ExpectedVersion) throw new DomainException("conflict"); if (p.Theme != theme || p.Locale != locale) { p.Theme = theme; p.Locale = locale; p.Version++; }
        return new PreferenceResult(p.Theme, p.Locale, p.Version);
    }, ct);
}
