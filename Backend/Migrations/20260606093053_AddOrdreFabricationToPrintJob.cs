using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdreFabricationToPrintJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrdreFabricationId",
                table: "PrintJobs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrdresFabrication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjetId = table.Column<int>(type: "int", nullable: false),
                    PieceId = table.Column<int>(type: "int", nullable: false),
                    Quantite = table.Column<int>(type: "int", nullable: false),
                    QuantiteProduite = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priorite = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdresFabrication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdresFabrication_Pieces_PieceId",
                        column: x => x.PieceId,
                        principalTable: "Pieces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrdresFabrication_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PrintProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrinterId = table.Column<int>(type: "int", nullable: false),
                    Materiau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NozzleTemp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BedTemp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LayerHeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Speed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Infill = table.Column<int>(type: "int", nullable: false),
                    InfillPattern = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supports = table.Column<bool>(type: "bit", nullable: false),
                    SupportType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintProfiles_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STLMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PieceId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SurfaceArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BoundingBoxX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BoundingBoxY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BoundingBoxZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedPrintTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TriangleCount = table.Column<int>(type: "int", nullable: false),
                    IsWatertight = table.Column<bool>(type: "bit", nullable: false),
                    HasErrors = table.Column<bool>(type: "bit", nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STLMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_STLMetadata_Pieces_PieceId",
                        column: x => x.PieceId,
                        principalTable: "Pieces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_OrdreFabricationId",
                table: "PrintJobs",
                column: "OrdreFabricationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdresFabrication_PieceId",
                table: "OrdresFabrication",
                column: "PieceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdresFabrication_ProjetId",
                table: "OrdresFabrication",
                column: "ProjetId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdresFabrication_Reference",
                table: "OrdresFabrication",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrintProfiles_PrinterId",
                table: "PrintProfiles",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_STLMetadata_PieceId",
                table: "STLMetadata",
                column: "PieceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintJobs_OrdresFabrication_OrdreFabricationId",
                table: "PrintJobs",
                column: "OrdreFabricationId",
                principalTable: "OrdresFabrication",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrintJobs_OrdresFabrication_OrdreFabricationId",
                table: "PrintJobs");

            migrationBuilder.DropTable(
                name: "OrdresFabrication");

            migrationBuilder.DropTable(
                name: "PrintProfiles");

            migrationBuilder.DropTable(
                name: "STLMetadata");

            migrationBuilder.DropIndex(
                name: "IX_PrintJobs_OrdreFabricationId",
                table: "PrintJobs");

            migrationBuilder.DropColumn(
                name: "OrdreFabricationId",
                table: "PrintJobs");

        }
    }
}
