using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Localization;
using IndicaAI.Web.Persistence;
using IndicaAI.Web.Text;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Web.Features.Tracking;

public sealed class StateLabelService(DataPaths paths)
{
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
