using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDevisIdToOrdreFabrication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DevisId",
                table: "OrdresFabrication",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdresFabrication_DevisId",
                table: "OrdresFabrication",
                column: "DevisId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdresFabrication_Devis_DevisId",
                table: "OrdresFabrication",
                column: "DevisId",
                principalTable: "Devis",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdresFabrication_Devis_DevisId",
                table: "OrdresFabrication");

            migrationBuilder.DropIndex(
                name: "IX_OrdresFabrication_DevisId",
                table: "OrdresFabrication");

            migrationBuilder.DropColumn(
                name: "DevisId",
                table: "OrdresFabrication");
        }
    }
}
