using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndicaAI.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogueCompatibilityTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TRIGGER CK_Games_INSERT BEFORE INSERT ON Games WHEN (SELECT MediaCode FROM Works WHERE Id=NEW.WorkId) <> 'jogo' BEGIN SELECT RAISE(ABORT, 'incompatible catalogue fields'); END;");
            migrationBuilder.Sql("CREATE TRIGGER CK_Games_UPDATE BEFORE UPDATE ON Games WHEN (SELECT MediaCode FROM Works WHERE Id=NEW.WorkId) <> 'jogo' BEGIN SELECT RAISE(ABORT, 'incompatible catalogue fields'); END;");
            migrationBuilder.Sql("CREATE TRIGGER CK_Films_INSERT BEFORE INSERT ON Films WHEN (SELECT MediaCode FROM Works WHERE Id=NEW.WorkId) <> 'filme' BEGIN SELECT RAISE(ABORT, 'incompatible catalogue fields'); END;");
            migrationBuilder.Sql("CREATE TRIGGER CK_Films_UPDATE BEFORE UPDATE ON Films WHEN (SELECT MediaCode FROM Works WHERE Id=NEW.WorkId) <> 'filme' BEGIN SELECT RAISE(ABORT, 'incompatible catalogue fields'); END;");
            migrationBuilder.Sql("CREATE TRIGGER CK_Credits_INSERT BEFORE INSERT ON Credits WHEN NOT EXISTS (SELECT 1 FROM Works w WHERE w.Id=NEW.WorkId AND ((w.MediaCode IN ('livro','manga') AND NEW.Role='author') OR (w.MediaCode='filme' AND NEW.Role='director') OR (w.MediaCode='serie' AND NEW.Role='creator'))) BEGIN SELECT RAISE(ABORT, 'incompatible catalogue fields'); END;");
            migrationBuilder.Sql("CREATE TRIGGER CK_Credits_UPDATE BEFORE UPDATE ON Credits WHEN NOT EXISTS (SELECT 1 FROM Works w WHERE w.Id=NEW.WorkId AND ((w.MediaCode IN ('livro','manga') AND NEW.Role='author') OR (w.MediaCode='filme' AND NEW.Role='director') OR (w.MediaCode='serie' AND NEW.Role='creator'))) BEGIN SELECT RAISE(ABORT, 'incompatible catalogue fields'); END;");
            migrationBuilder.Sql("CREATE TRIGGER CK_Work_SpecificMedia BEFORE UPDATE OF MediaCode ON Works WHEN (NEW.MediaCode <> 'jogo' AND EXISTS(SELECT 1 FROM Games WHERE WorkId=NEW.Id)) OR (NEW.MediaCode <> 'filme' AND EXISTS(SELECT 1 FROM Films WHERE WorkId=NEW.Id)) OR EXISTS(SELECT 1 FROM Credits c WHERE c.WorkId=NEW.Id AND NOT ((NEW.MediaCode IN ('livro','manga') AND c.Role='author') OR (NEW.MediaCode='filme' AND c.Role='director') OR (NEW.MediaCode='serie' AND c.Role='creator'))) BEGIN SELECT RAISE(ABORT, 'incompatible catalogue fields'); END;");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CK_Games_INSERT");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CK_Games_UPDATE");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CK_Films_INSERT");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CK_Films_UPDATE");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CK_Credits_INSERT");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CK_Credits_UPDATE");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CK_Work_SpecificMedia");

        }
    }
}

