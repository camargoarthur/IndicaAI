using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IndicaAI.Web.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261120162500_TmdbSeriesIdentity")]
public sealed class TmdbSeriesIdentity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "TmdbSeriesId", table: "Works", type: "INTEGER", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_Works_TmdbSeriesId", table: "Works", column: "TmdbSeriesId", unique: true);
        migrationBuilder.Sql("""
            CREATE TRIGGER TR_Works_TmdbSeriesType_Insert BEFORE INSERT ON Works
            WHEN NEW.TmdbSeriesId IS NOT NULL AND (NEW.MediaCode <> 'serie' OR NEW.TmdbSeriesId <= 0)
            BEGIN SELECT RAISE(ABORT, 'TMDB series identity requires a series and positive ID'); END;
            CREATE TRIGGER TR_Works_TmdbSeriesType_Update BEFORE UPDATE OF MediaCode, TmdbSeriesId ON Works
            WHEN NEW.TmdbSeriesId IS NOT NULL AND (NEW.MediaCode <> 'serie' OR NEW.TmdbSeriesId <= 0)
            BEGIN SELECT RAISE(ABORT, 'TMDB series identity requires a series and positive ID'); END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER TR_Works_TmdbSeriesType_Insert; DROP TRIGGER TR_Works_TmdbSeriesType_Update;");
        migrationBuilder.DropIndex(name: "IX_Works_TmdbSeriesId", table: "Works");
        migrationBuilder.Sql("ALTER TABLE \"Works\" DROP COLUMN \"TmdbSeriesId\";");
    }
}
