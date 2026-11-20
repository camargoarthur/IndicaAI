using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndicaAI.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLocalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameKey = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Identities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", nullable: false),
                    PlanHash = table.Column<string>(type: "TEXT", nullable: false),
                    ContractVersion = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    BaselineRevision = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Code);
                    table.CheckConstraint("CK_Media_Code", "Code IN ('jogo','livro','manga','filme','serie')");
                });

            migrationBuilder.CreateTable(
                name: "Revisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    GroupId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameKey = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IdentityId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lists", x => x.Id);
                    table.UniqueConstraint("AK_Lists_IdentityId_Id", x => new { x.IdentityId, x.Id });
                    table.ForeignKey(
                        name: "FK_Lists_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    IdentityId = table.Column<string>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false),
                    Locale = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.IdentityId);
                    table.CheckConstraint("CK_Preference_Theme", "Theme IN ('light','dark')");
                    table.ForeignKey(
                        name: "FK_Preferences_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    IdentityId = table.Column<string>(type: "TEXT", nullable: false),
                    OperationId = table.Column<string>(type: "TEXT", nullable: false),
                    RequestHash = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    Result = table.Column<string>(type: "TEXT", nullable: false),
                    RecordedAtUtc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => new { x.IdentityId, x.OperationId });
                    table.ForeignKey(
                        name: "FK_Receipts_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportRows",
                columns: table => new
                {
                    BatchId = table.Column<string>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    PhysicalStartLine = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ReasonCode = table.Column<string>(type: "TEXT", nullable: true),
                    ReasonArgs = table.Column<string>(type: "TEXT", nullable: true),
                    Before = table.Column<string>(type: "TEXT", nullable: true),
                    After = table.Column<string>(type: "TEXT", nullable: true),
                    Preserved = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRows", x => new { x.BatchId, x.RecordNumber });
                    table.ForeignKey(
                        name: "FK_ImportRows_ImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaseStates",
                columns: table => new
                {
                    MediaCode = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseStates", x => new { x.MediaCode, x.Code });
                    table.ForeignKey(
                        name: "FK_BaseStates_Media_MediaCode",
                        column: x => x.MediaCode,
                        principalTable: "Media",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Works",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MediaCode = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseYear = table.Column<int>(type: "INTEGER", nullable: true),
                    IsAnime = table.Column<bool>(type: "INTEGER", nullable: false),
                    CoverAsset = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Works", x => x.Id);
                    table.UniqueConstraint("AK_Works_Id_MediaCode", x => new { x.Id, x.MediaCode });
                    table.CheckConstraint("CK_Work_Anime", "IsAnime = 0 OR MediaCode IN ('filme','serie')");
                    table.CheckConstraint("CK_Work_Title", "length(trim(Title)) > 0");
                    table.ForeignKey(
                        name: "FK_Works_Media_MediaCode",
                        column: x => x.MediaCode,
                        principalTable: "Media",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TagMedia",
                columns: table => new
                {
                    TagId = table.Column<string>(type: "TEXT", nullable: false),
                    MediaCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagMedia", x => new { x.TagId, x.MediaCode });
                    table.ForeignKey(
                        name: "FK_TagMedia_Media_MediaCode",
                        column: x => x.MediaCode,
                        principalTable: "Media",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TagMedia_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StateLabels",
                columns: table => new
                {
                    IdentityId = table.Column<string>(type: "TEXT", nullable: false),
                    MediaCode = table.Column<string>(type: "TEXT", nullable: false),
                    StateCode = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    LabelKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateLabels", x => new { x.IdentityId, x.MediaCode, x.StateCode });
                    table.ForeignKey(
                        name: "FK_StateLabels_BaseStates_MediaCode_StateCode",
                        columns: x => new { x.MediaCode, x.StateCode },
                        principalTable: "BaseStates",
                        principalColumns: new[] { "MediaCode", "Code" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StateLabels_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Credits",
                columns: table => new
                {
                    WorkId = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credits", x => new { x.WorkId, x.Role, x.Position });
                    table.ForeignKey(
                        name: "FK_Credits_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Films",
                columns: table => new
                {
                    WorkId = table.Column<string>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Films", x => x.WorkId);
                    table.CheckConstraint("CK_Film_Duration", "DurationMinutes IS NULL OR DurationMinutes > 0");
                    table.ForeignKey(
                        name: "FK_Films_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    WorkId = table.Column<string>(type: "TEXT", nullable: false),
                    Studio = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.WorkId);
                    table.ForeignKey(
                        name: "FK_Games_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trackings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IdentityId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkId = table.Column<string>(type: "TEXT", nullable: false),
                    MediaCode = table.Column<string>(type: "TEXT", nullable: false),
                    StateCode = table.Column<string>(type: "TEXT", nullable: false),
                    IncludedAtUtc = table.Column<string>(type: "TEXT", nullable: false),
                    IncludedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    ArchivedAtUtc = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trackings", x => x.Id);
                    table.UniqueConstraint("AK_Trackings_IdentityId_Id", x => new { x.IdentityId, x.Id });
                    table.ForeignKey(
                        name: "FK_Trackings_BaseStates_MediaCode_StateCode",
                        columns: x => new { x.MediaCode, x.StateCode },
                        principalTable: "BaseStates",
                        principalColumns: new[] { "MediaCode", "Code" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trackings_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trackings_Works_WorkId_MediaCode",
                        columns: x => new { x.WorkId, x.MediaCode },
                        principalTable: "Works",
                        principalColumns: new[] { "Id", "MediaCode" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkTags",
                columns: table => new
                {
                    WorkId = table.Column<string>(type: "TEXT", nullable: false),
                    TagId = table.Column<string>(type: "TEXT", nullable: false),
                    MediaCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTags", x => new { x.WorkId, x.TagId });
                    table.ForeignKey(
                        name: "FK_WorkTags_TagMedia_TagId_MediaCode",
                        columns: x => new { x.TagId, x.MediaCode },
                        principalTable: "TagMedia",
                        principalColumns: new[] { "TagId", "MediaCode" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkTags_Works_WorkId_MediaCode",
                        columns: x => new { x.WorkId, x.MediaCode },
                        principalTable: "Works",
                        principalColumns: new[] { "Id", "MediaCode" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    WorkId = table.Column<string>(type: "TEXT", nullable: false),
                    NameKey = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => new { x.WorkId, x.NameKey });
                    table.ForeignKey(
                        name: "FK_Platforms_Games_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Games",
                        principalColumn: "WorkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cycles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    TrackingId = table.Column<string>(type: "TEXT", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<string>(type: "TEXT", nullable: false),
                    StartOn = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CompletionOn = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cycles", x => x.Id);
                    table.UniqueConstraint("AK_Cycles_TrackingId_Id", x => new { x.TrackingId, x.Id });
                    table.ForeignKey(
                        name: "FK_Cycles_Trackings_TrackingId",
                        column: x => x.TrackingId,
                        principalTable: "Trackings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListItems",
                columns: table => new
                {
                    ListId = table.Column<string>(type: "TEXT", nullable: false),
                    TrackingId = table.Column<string>(type: "TEXT", nullable: false),
                    IdentityId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListItems", x => new { x.ListId, x.TrackingId });
                    table.ForeignKey(
                        name: "FK_ListItems_Lists_IdentityId_ListId",
                        columns: x => new { x.IdentityId, x.ListId },
                        principalTable: "Lists",
                        principalColumns: new[] { "IdentityId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListItems_Trackings_IdentityId_TrackingId",
                        columns: x => new { x.IdentityId, x.TrackingId },
                        principalTable: "Trackings",
                        principalColumns: new[] { "IdentityId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    TrackingId = table.Column<string>(type: "TEXT", nullable: false),
                    CycleId = table.Column<string>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    RecordedAtUtc = table.Column<string>(type: "TEXT", nullable: false),
                    MediaCode = table.Column<string>(type: "TEXT", nullable: false),
                    FromStateCode = table.Column<string>(type: "TEXT", nullable: true),
                    ToStateCode = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectedEventId = table.Column<string>(type: "TEXT", nullable: true),
                    Field = table.Column<string>(type: "TEXT", nullable: true),
                    OldValue = table.Column<string>(type: "TEXT", nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", nullable: true),
                    OperationId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Cycles_TrackingId_CycleId",
                        columns: x => new { x.TrackingId, x.CycleId },
                        principalTable: "Cycles",
                        principalColumns: new[] { "TrackingId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cycles_TrackingId_Ordinal",
                table: "Cycles",
                columns: new[] { "TrackingId", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_TrackingId_CycleId",
                table: "Events",
                columns: new[] { "TrackingId", "CycleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_TrackingId_Sequence",
                table: "Events",
                columns: new[] { "TrackingId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_NameKey",
                table: "Groups",
                column: "NameKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListItems_IdentityId_ListId",
                table: "ListItems",
                columns: new[] { "IdentityId", "ListId" });

            migrationBuilder.CreateIndex(
                name: "IX_ListItems_IdentityId_TrackingId",
                table: "ListItems",
                columns: new[] { "IdentityId", "TrackingId" });

            migrationBuilder.CreateIndex(
                name: "IX_StateLabels_IdentityId_MediaCode_LabelKey",
                table: "StateLabels",
                columns: new[] { "IdentityId", "MediaCode", "LabelKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StateLabels_MediaCode_StateCode",
                table: "StateLabels",
                columns: new[] { "MediaCode", "StateCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TagMedia_MediaCode",
                table: "TagMedia",
                column: "MediaCode");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GroupId_NameKey",
                table: "Tags",
                columns: new[] { "GroupId", "NameKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trackings_IdentityId_ArchivedAtUtc_MediaCode",
                table: "Trackings",
                columns: new[] { "IdentityId", "ArchivedAtUtc", "MediaCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Trackings_IdentityId_WorkId",
                table: "Trackings",
                columns: new[] { "IdentityId", "WorkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trackings_MediaCode_StateCode",
                table: "Trackings",
                columns: new[] { "MediaCode", "StateCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Trackings_WorkId_MediaCode",
                table: "Trackings",
                columns: new[] { "WorkId", "MediaCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Works_MediaCode",
                table: "Works",
                column: "MediaCode");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTags_TagId_MediaCode",
                table: "WorkTags",
                columns: new[] { "TagId", "MediaCode" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTags_WorkId_MediaCode",
                table: "WorkTags",
                columns: new[] { "WorkId", "MediaCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credits");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Films");

            migrationBuilder.DropTable(
                name: "ImportRows");

            migrationBuilder.DropTable(
                name: "ListItems");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "Revisions");

            migrationBuilder.DropTable(
                name: "StateLabels");

            migrationBuilder.DropTable(
                name: "WorkTags");

            migrationBuilder.DropTable(
                name: "Cycles");

            migrationBuilder.DropTable(
                name: "ImportBatches");

            migrationBuilder.DropTable(
                name: "Lists");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "TagMedia");

            migrationBuilder.DropTable(
                name: "Trackings");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "BaseStates");

            migrationBuilder.DropTable(
                name: "Identities");

            migrationBuilder.DropTable(
                name: "Works");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Media");
        }
    }
}
