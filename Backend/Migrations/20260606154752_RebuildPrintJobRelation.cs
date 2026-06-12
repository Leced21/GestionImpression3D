using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RebuildPrintJobRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('CommandeLignes', 'CommandeId1') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.foreign_keys
                        WHERE name = 'FK_CommandeLignes_Commandes_CommandeId1'
                    )
                    BEGIN
                        ALTER TABLE [CommandeLignes] DROP CONSTRAINT [FK_CommandeLignes_Commandes_CommandeId1];
                    END

                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = 'IX_CommandeLignes_CommandeId1'
                          AND object_id = OBJECT_ID(N'[CommandeLignes]')
                    )
                    BEGIN
                        DROP INDEX [IX_CommandeLignes_CommandeId1] ON [CommandeLignes];
                    END

                    ALTER TABLE [CommandeLignes] DROP COLUMN [CommandeId1];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
