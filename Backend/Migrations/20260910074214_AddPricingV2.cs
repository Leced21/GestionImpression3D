using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ElectricityPricePerKwh",
                table: "UserSettings",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "PieceCostings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PieceId = table.Column<int>(type: "int", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuantityKg = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FilamentPricePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PrintTimeHours = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ModelingTimeHours = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PostProcessingTimeHours = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RealSalePriceHt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieceCostings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PieceCostings_Pieces_PieceId",
                        column: x => x.PieceId,
                        principalTable: "Pieces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ElectricityPricePerKwh = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AveragePrinterPowerKw = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrinterPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductiveLifetimeHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaborHourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WasteRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PackagingCostPerPiece = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsumablesCostPerPiece = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellingFeesRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TargetGrossMarginRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PlaPricePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbsPricePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PetgPricePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TpuPricePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PieceCostings_PieceId",
                table: "PieceCostings",
                column: "PieceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PieceCostings");

            migrationBuilder.DropTable(
                name: "PricingSettings");

            migrationBuilder.AlterColumn<decimal>(
                name: "ElectricityPricePerKwh",
                table: "UserSettings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
