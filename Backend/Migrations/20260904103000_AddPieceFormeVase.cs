using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPieceFormeVase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Pieces', 'FormeVase') IS NULL
                BEGIN
                    ALTER TABLE [Pieces]
                    ADD [FormeVase] nvarchar(50) NULL
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Pieces', 'FormeVase') IS NOT NULL
                BEGIN
                    ALTER TABLE [Pieces] DROP COLUMN [FormeVase]
                END
                """);
        }
    }
}
