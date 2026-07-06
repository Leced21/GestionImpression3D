using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFicheProduitFieldsToPiece : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CapaciteContenance",
                table: "Pieces",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conditionnement",
                table: "Pieces",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Couleurs",
                table: "Pieces",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DelaiLivraisonJours",
                table: "Pieces",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DimensionsColis",
                table: "Pieces",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Faq",
                table: "Pieces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructionsUtilisation",
                table: "Pieces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MoqUnites",
                table: "Pieces",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormesCertifications",
                table: "Pieces",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PoidsColisKg",
                table: "Pieces",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PointsForts",
                table: "Pieces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrecautionsUsage",
                table: "Pieces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicCible",
                table: "Pieces",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TarifsDegressifs",
                table: "Pieces",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapaciteContenance",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "Conditionnement",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "Couleurs",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "DelaiLivraisonJours",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "DimensionsColis",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "Faq",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "InstructionsUtilisation",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "MoqUnites",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "NormesCertifications",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "PoidsColisKg",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "PointsForts",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "PrecautionsUsage",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "PublicCible",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "TarifsDegressifs",
                table: "Pieces");
        }
    }
}
