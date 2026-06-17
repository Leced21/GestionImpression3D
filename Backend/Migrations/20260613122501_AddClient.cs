using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Commandes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CodePostal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ville = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pays = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Siret = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    TVAIntra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrintIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrintJobId = table.Column<int>(type: "int", nullable: true),
                    PrinterId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportedBy = table.Column<int>(type: "int", nullable: true),
                    ResolvedBy = table.Column<int>(type: "int", nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: true),
                    ResolvedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintIncidents_PrintJobs_PrintJobId",
                        column: x => x.PrintJobId,
                        principalTable: "PrintJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PrintIncidents_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PrintIncidents_Users_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PrintIncidents_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Devis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroDevis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ProjetId = table.Column<int>(type: "int", nullable: true),
                    DateEmission = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateValidite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devis_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Devis_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevisLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevisId = table.Column<int>(type: "int", nullable: false),
                    PieceId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantite = table.Column<int>(type: "int", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevisLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevisLignes_Devis_DevisId",
                        column: x => x.DevisId,
                        principalTable: "Devis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevisLignes_Pieces_PieceId",
                        column: x => x.PieceId,
                        principalTable: "Pieces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_ClientId",
                table: "Commandes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devis_ClientId",
                table: "Devis",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Devis_NumeroDevis",
                table: "Devis",
                column: "NumeroDevis",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devis_ProjetId",
                table: "Devis",
                column: "ProjetId");

            migrationBuilder.CreateIndex(
                name: "IX_DevisLignes_DevisId",
                table: "DevisLignes",
                column: "DevisId");

            migrationBuilder.CreateIndex(
                name: "IX_DevisLignes_PieceId",
                table: "DevisLignes",
                column: "PieceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_OccurredAt",
                table: "PrintIncidents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_PrinterId",
                table: "PrintIncidents",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_PrintJobId",
                table: "PrintIncidents",
                column: "PrintJobId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_ReportedByUserId",
                table: "PrintIncidents",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_ResolvedByUserId",
                table: "PrintIncidents",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_Status",
                table: "PrintIncidents",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Commandes_Clients_ClientId",
                table: "Commandes",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_Clients_ClientId",
                table: "Commandes");

            migrationBuilder.DropTable(
                name: "DevisLignes");

            migrationBuilder.DropTable(
                name: "PrintIncidents");

            migrationBuilder.DropTable(
                name: "Devis");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Commandes_ClientId",
                table: "Commandes");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Commandes");
        }
    }
}
