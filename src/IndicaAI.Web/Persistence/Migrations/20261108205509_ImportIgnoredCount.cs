using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndicaAI.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImportIgnoredCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IgnoredRecords",
                table: "ImportBatches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IgnoredRecords",
                table: "ImportBatches");
        }
    }
}
