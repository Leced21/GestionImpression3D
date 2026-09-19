using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class _20260904110000_AddEnergyCostSettings : Migration
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

            migrationBuilder.Sql("""
                IF COL_LENGTH('UserSettings', 'ElectricityPricePerKwh') IS NULL
                BEGIN
                    ALTER TABLE [UserSettings]
                    ADD [ElectricityPricePerKwh] decimal(18,4) NOT NULL
                    CONSTRAINT [DF_UserSettings_ElectricityPricePerKwh] DEFAULT 0.25
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Printers', 'PowerWatts') IS NULL
                BEGIN
                    ALTER TABLE [Printers]
                    ADD [PowerWatts] decimal(18,2) NOT NULL
                    CONSTRAINT [DF_Printers_PowerWatts] DEFAULT 120
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Printers', 'PowerWatts') IS NOT NULL
                BEGIN
                    ALTER TABLE [Printers] DROP CONSTRAINT IF EXISTS [DF_Printers_PowerWatts]
                    ALTER TABLE [Printers] DROP COLUMN [PowerWatts]
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('UserSettings', 'ElectricityPricePerKwh') IS NOT NULL
                BEGIN
                    ALTER TABLE [UserSettings] DROP CONSTRAINT IF EXISTS [DF_UserSettings_ElectricityPricePerKwh]
                    ALTER TABLE [UserSettings] DROP COLUMN [ElectricityPricePerKwh]
                END
                """);
        }
    }
}
