using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndicaAI.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCatalogueFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Credit_Value",
                table: "Credits",
                sql: "Position > 0 AND length(trim(Name)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_State_Code",
                table: "BaseStates",
                sql: "Code IN ('planned','in_progress','paused','completed','dropped')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Credit_Value",
                table: "Credits");

            migrationBuilder.DropCheckConstraint(
                name: "CK_State_Code",
                table: "BaseStates");
        }
    }
}
