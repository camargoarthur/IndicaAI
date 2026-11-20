using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IndicaAI.Web.Persistence;

public sealed class AppDbContext(DataPaths paths, params IInterceptor[] interceptors) : DbContext
{
    public DbSet<LocalIdentity> Identities => Set<LocalIdentity>();
    public DbSet<UserPreference> Preferences => Set<UserPreference>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<BaseState> BaseStates => Set<BaseState>();
    public DbSet<Work> Works => Set<Work>();
    public DbSet<WorkCredit> Credits => Set<WorkCredit>();
    public DbSet<GameDetail> Games => Set<GameDetail>();
    public DbSet<GamePlatform> Platforms => Set<GamePlatform>();
    public DbSet<FilmDetail> Films => Set<FilmDetail>();
    public DbSet<Tracking> Trackings => Set<Tracking>();
    public DbSet<ConsumptionCycle> Cycles => Set<ConsumptionCycle>();
    public DbSet<TrackingEvent> Events => Set<TrackingEvent>();
    public DbSet<StateLabel> StateLabels => Set<StateLabel>();
    public DbSet<PersonalList> Lists => Set<PersonalList>();
    public DbSet<ListItem> ListItems => Set<ListItem>();
    public DbSet<TagGroup> Groups => Set<TagGroup>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TagMedia> TagMedia => Set<TagMedia>();
    public DbSet<WorkTag> WorkTags => Set<WorkTag>();
    public DbSet<ClassificationRevision> Revisions => Set<ClassificationRevision>();
    public DbSet<OperationReceipt> Receipts => Set<OperationReceipt>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportRow> ImportRows => Set<ImportRow>();
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlite(new SqliteConnectionStringBuilder { DataSource = paths.Database, ForeignKeys = true, Pooling = false, DefaultTimeout = 10 }.ToString());
        if (interceptors.Length > 0) builder.AddInterceptors(interceptors);
    }
    protected override void OnModelCreating(ModelBuilder b) => ModelConfiguration.Configure(b);
}
