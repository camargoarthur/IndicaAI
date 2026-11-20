using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndicaAI.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TmdbMovieIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TmdbMovieId",
                table: "Works",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Works_TmdbMovieId",
                table: "Works",
                column: "TmdbMovieId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Works_TmdbMovieId",
                table: "Works");

            // SQLite can drop this unreferenced column without rebuilding Works and breaking cross-table triggers.
            migrationBuilder.Sql("ALTER TABLE \"Works\" DROP COLUMN \"TmdbMovieId\";");
        }
    }
}
