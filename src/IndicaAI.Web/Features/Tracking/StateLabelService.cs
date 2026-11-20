using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Localization;
using IndicaAI.Web.Persistence;
using IndicaAI.Web.Text;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Tracking;

public sealed class StateLabelService(DataPaths paths)
{
    public Task<long> SaveMediaAsync(Mutation mutation, string media, IReadOnlyDictionary<string, string?> labels, IEnumerable<string> resetCodes, CancellationToken ct = default)
    {
        var changes = labels.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
        var resets = resetCodes.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        return new CommandRunner(paths).RunAsync(mutation.OperationId, "state.labels", new { mutation, media, changes, resets }, async (db, owner) =>
        {
            var codes = await db.BaseStates.Where(s => s.MediaCode == media).Select(s => s.Code).ToArrayAsync(ct);
            if (codes.Length == 0 || changes.Any(pair => !codes.Contains(pair.Key)) || resets.Any(code => !codes.Contains(code)))
                throw new DomainException("state_invalid");

            var preference = await db.Preferences.SingleAsync(p => p.IdentityId == owner, ct);
            if (preference.Version != mutation.ExpectedVersion) throw new DomainException("conflict");
            var rows = db.StateLabels.Where(l => l.IdentityId == owner && l.MediaCode == media);
            var currentLabels = await rows.AsNoTracking().ToDictionaryAsync(l => l.StateCode, l => l.Label, ct);
            var defaults = codes.ToDictionary(code => code, code => UiMessages.State(media, code, preference.Locale));
            var nextLabels = codes.ToDictionary(code => code, code => currentLabels.GetValueOrDefault(code));
            foreach (var (code, value) in changes)
            {
                if (resets.Contains(code)) continue;
                var label = value?.Trim() ?? "";
                if (label == (nextLabels[code] ?? defaults[code])) continue;
                if (label.Length == 0) throw new DomainException("name_required");
                if (label.Length > 100) throw new DomainException("label_too_long");
                nextLabels[code] = label;
            }
            foreach (var code in resets) nextLabels[code] = null;

            var updatedCodes = codes.Where(code => currentLabels.GetValueOrDefault(code) != nextLabels[code]).ToArray();
            if (updatedCodes.Length == 0) return preference.Version;
            var effectiveLabels = codes.GroupBy(code => UnicodeNameKey.Create(nextLabels[code] ?? defaults[code]));
            if (effectiveLabels.Any(group => group.Count() > 1 && group.Any(code => updatedCodes.Contains(code))))
                throw new DomainException("name_duplicate");

            // Release the old unique keys before inserting the new labels, including when two labels trade names.
            await rows.Where(l => updatedCodes.Contains(l.StateCode)).ExecuteDeleteAsync(ct);
            foreach (var code in updatedCodes)
            {
                if (nextLabels[code] is not { } label) continue;
                db.StateLabels.Add(new() { IdentityId = owner, MediaCode = media, StateCode = code, Label = label, LabelKey = UnicodeNameKey.Create(label) });
            }
            return ++preference.Version;
        }, ct);
    }

    public Task<long> RenameAsync(Mutation mutation, string media, string code, string? label, CancellationToken ct = default) =>
        new CommandRunner(paths).RunAsync(mutation.OperationId, "state.label", new { mutation, media, code, label }, async (db, owner) =>
        {
            if (!await db.BaseStates.AnyAsync(s => s.MediaCode == media && s.Code == code, ct)) throw new DomainException("state_invalid");
            var pref = await db.Preferences.SingleAsync(p => p.IdentityId == owner, ct);
            if (pref.Version != mutation.ExpectedVersion) throw new DomainException("conflict");
            var item = await db.StateLabels.FindAsync([owner, media, code], ct);
            if (label is null) { if (item is not null) db.StateLabels.Remove(item); }
            else
            {
                label = label.Trim(); if (label.Length == 0) throw new DomainException("name_required");
                var key = UnicodeNameKey.Create(label);
                if (await db.StateLabels.AnyAsync(x => x.IdentityId == owner && x.MediaCode == media && x.StateCode != code && x.LabelKey == key, ct)) throw new DomainException("name_duplicate");
                foreach (var other in MediaCodes.States.Where(c => c != code))
                {
                    var custom = await db.StateLabels.FindAsync([owner, media, other], ct);
                    if (custom is null && UnicodeNameKey.Create(UiMessages.State(media, other, pref.Locale)) == key) throw new DomainException("name_duplicate");
                }
                if (item is null) db.StateLabels.Add(new() { IdentityId = owner, MediaCode = media, StateCode = code, Label = label, LabelKey = key });
                else { item.Label = label; item.LabelKey = key; }
            }
            return ++pref.Version;
        }, ct);
}
