using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialStockId = table.Column<int>(type: "int", nullable: false),
                    PrintJobId = table.Column<int>(type: "int", nullable: true),
                    OrdreFabricationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsumedBy = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialConsumptions_MaterialStocks_MaterialStockId",
                        column: x => x.MaterialStockId,
                        principalTable: "MaterialStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialConsumptions_OrdresFabrication_OrdreFabricationId",
                        column: x => x.OrdreFabricationId,
                        principalTable: "OrdresFabrication",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialConsumptions_PrintJobs_PrintJobId",
                        column: x => x.PrintJobId,
                        principalTable: "PrintJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialConsumptions_Users_ConsumedBy",
                        column: x => x.ConsumedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PrinterMaintenances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrinterId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterMaintenances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrinterMaintenances_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptions_ConsumedAt",
                table: "MaterialConsumptions",
                column: "ConsumedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptions_ConsumedBy",
                table: "MaterialConsumptions",
                column: "ConsumedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptions_MaterialStockId",
                table: "MaterialConsumptions",
                column: "MaterialStockId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptions_OrdreFabricationId",
                table: "MaterialConsumptions",
                column: "OrdreFabricationId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptions_PrintJobId",
                table: "MaterialConsumptions",
                column: "PrintJobId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialConsumptions_Type",
                table: "MaterialConsumptions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterMaintenances_PrinterId",
                table: "PrinterMaintenances",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterMaintenances_ScheduledDate",
                table: "PrinterMaintenances",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterMaintenances_Status",
                table: "PrinterMaintenances",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialConsumptions");

            migrationBuilder.DropTable(
                name: "PrinterMaintenances");
        }
    }
}
