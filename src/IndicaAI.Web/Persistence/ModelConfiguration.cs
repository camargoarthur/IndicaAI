using IndicaAI.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace IndicaAI.Web.Persistence;

internal static class ModelConfiguration
{
    public static void Configure(ModelBuilder b)
    {
        b.Entity<LocalIdentity>().HasKey(x => x.Id);
        b.Entity<UserPreference>().HasKey(x => x.IdentityId);
        b.Entity<UserPreference>().HasOne<LocalIdentity>().WithOne().HasForeignKey<UserPreference>(x => x.IdentityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<UserPreference>().ToTable(t => t.HasCheckConstraint("CK_Preference_Theme", "Theme IN ('light','dark')"));
        b.Entity<Media>().HasKey(x => x.Code);
        b.Entity<Media>().ToTable(t => t.HasCheckConstraint("CK_Media_Code", "Code IN ('jogo','livro','manga','filme','serie')"));
        b.Entity<BaseState>().HasKey(x => new { x.MediaCode, x.Code });
        b.Entity<BaseState>().HasOne<Media>().WithMany().HasForeignKey(x => x.MediaCode).OnDelete(DeleteBehavior.Restrict);
        b.Entity<BaseState>().ToTable(t=>t.HasCheckConstraint("CK_State_Code","Code IN ('planned','in_progress','paused','completed','dropped')"));
        b.Entity<Work>().HasKey(x => x.Id);
        b.Entity<Work>().HasAlternateKey(x => new { x.Id, x.MediaCode });
        b.Entity<Work>().HasOne<Media>().WithMany().HasForeignKey(x => x.MediaCode).OnDelete(DeleteBehavior.Restrict);
        b.Entity<WorkCredit>().ToTable(t=>t.HasCheckConstraint("CK_Credit_Value","Position > 0 AND length(trim(Name)) > 0"));
        b.Entity<FilmDetail>().ToTable(t=>t.HasCheckConstraint("CK_Film_Duration","DurationMinutes IS NULL OR DurationMinutes > 0"));
        b.Entity<Work>().ToTable(t => { t.HasCheckConstraint("CK_Work_Anime", "IsAnime = 0 OR MediaCode IN ('filme','serie')"); t.HasCheckConstraint("CK_Work_Title", "length(trim(Title)) > 0"); });
        b.Entity<WorkCredit>().HasKey(x => new { x.WorkId, x.Role, x.Position });
        b.Entity<WorkCredit>().HasOne<Work>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<GameDetail>().HasKey(x => x.WorkId);
        b.Entity<GameDetail>().HasOne<Work>().WithOne().HasForeignKey<GameDetail>(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<GamePlatform>().HasKey(x => new { x.WorkId, x.NameKey });
        b.Entity<GamePlatform>().HasOne<GameDetail>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<FilmDetail>().HasKey(x => x.WorkId);
        b.Entity<FilmDetail>().HasOne<Work>().WithOne().HasForeignKey<FilmDetail>(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<FilmDetail>().ToTable(t => t.HasCheckConstraint("CK_Film_Duration", "DurationMinutes IS NULL OR DurationMinutes > 0"));
        b.Entity<Tracking>().HasKey(x => x.Id);
        b.Entity<Tracking>().HasAlternateKey(x => new { x.IdentityId, x.Id });
        b.Entity<Tracking>().HasIndex(x => new { x.IdentityId, x.WorkId }).IsUnique();
        b.Entity<Tracking>().HasIndex(x => new { x.IdentityId, x.ArchivedAtUtc, x.MediaCode });
        b.Entity<Tracking>().HasOne<LocalIdentity>().WithMany().HasForeignKey(x => x.IdentityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Tracking>().HasOne<Work>().WithMany().HasForeignKey(x => new { x.WorkId, x.MediaCode }).HasPrincipalKey(x => new { x.Id, x.MediaCode }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Tracking>().HasOne<BaseState>().WithMany().HasForeignKey(x => new { x.MediaCode, Code = x.StateCode }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ConsumptionCycle>().HasKey(x => x.Id);
        b.Entity<ConsumptionCycle>().HasAlternateKey(x => new { x.TrackingId, x.Id });
        b.Entity<ConsumptionCycle>().HasIndex(x => new { x.TrackingId, x.Ordinal }).IsUnique();
        b.Entity<ConsumptionCycle>().HasOne<Tracking>().WithMany().HasForeignKey(x => x.TrackingId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TrackingEvent>().HasKey(x => x.Id);
        b.Entity<TrackingEvent>().HasIndex(x => new { x.TrackingId, x.Sequence }).IsUnique();
        b.Entity<TrackingEvent>().HasOne<ConsumptionCycle>().WithMany().HasForeignKey(x => new { x.TrackingId, Id = x.CycleId }).HasPrincipalKey(x => new { x.TrackingId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<StateLabel>().HasKey(x => new { x.IdentityId, x.MediaCode, x.StateCode });
        b.Entity<StateLabel>().HasIndex(x => new { x.IdentityId, x.MediaCode, x.LabelKey }).IsUnique();
        b.Entity<StateLabel>().HasOne<LocalIdentity>().WithMany().HasForeignKey(x => x.IdentityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<StateLabel>().HasOne<BaseState>().WithMany().HasForeignKey(x => new { x.MediaCode, Code = x.StateCode }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PersonalList>().HasKey(x => x.Id);
        b.Entity<PersonalList>().HasAlternateKey(x => new { x.IdentityId, x.Id });
        b.Entity<PersonalList>().HasOne<LocalIdentity>().WithMany().HasForeignKey(x => x.IdentityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ListItem>().HasKey(x => new { x.ListId, x.TrackingId });
        b.Entity<ListItem>().HasOne<PersonalList>().WithMany().HasForeignKey(x => new { x.IdentityId, Id = x.ListId }).HasPrincipalKey(x => new { x.IdentityId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ListItem>().HasOne<Tracking>().WithMany().HasForeignKey(x => new { x.IdentityId, Id = x.TrackingId }).HasPrincipalKey(x => new { x.IdentityId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TagGroup>().HasKey(x => x.Id);
        b.Entity<TagGroup>().HasIndex(x => x.NameKey).IsUnique();
        b.Entity<Tag>().HasKey(x => x.Id);
        b.Entity<Tag>().HasIndex(x => new { x.GroupId, x.NameKey }).IsUnique();
        b.Entity<Tag>().HasOne<TagGroup>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TagMedia>().HasKey(x => new { x.TagId, x.MediaCode });
        b.Entity<TagMedia>().HasOne<Tag>().WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TagMedia>().HasOne<Media>().WithMany().HasForeignKey(x => x.MediaCode).OnDelete(DeleteBehavior.Restrict);
        b.Entity<WorkTag>().HasKey(x => new { x.WorkId, x.TagId });
        b.Entity<WorkTag>().HasOne<Work>().WithMany().HasForeignKey(x => new { x.WorkId, x.MediaCode }).HasPrincipalKey(x => new { x.Id, x.MediaCode }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<WorkTag>().HasOne<TagMedia>().WithMany().HasForeignKey(x => new { x.TagId, x.MediaCode }).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ClassificationRevision>().HasKey(x => x.Id);
        b.Entity<OperationReceipt>().HasKey(x => new { x.IdentityId, x.OperationId });
        b.Entity<OperationReceipt>().HasOne<LocalIdentity>().WithMany().HasForeignKey(x => x.IdentityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ImportBatch>().HasKey(x => x.Id);
        b.Entity<ImportRow>().HasKey(x => new { x.BatchId, x.RecordNumber });
        b.Entity<ImportRow>().HasOne<ImportBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        foreach (var entity in b.Model.GetEntityTypes())
        {
            var version = entity.FindProperty("Version");
            if (version is not null) version.IsConcurrencyToken = true;
        }
    }
}
