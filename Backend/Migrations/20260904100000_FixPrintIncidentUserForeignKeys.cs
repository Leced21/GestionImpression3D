using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixPrintIncidentUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrintIncidents_Users_ReportedByUserId",
                table: "PrintIncidents");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintIncidents_Users_ResolvedByUserId",
                table: "PrintIncidents");

            migrationBuilder.DropIndex(
                name: "IX_PrintIncidents_ReportedByUserId",
                table: "PrintIncidents");

            migrationBuilder.DropIndex(
                name: "IX_PrintIncidents_ResolvedByUserId",
                table: "PrintIncidents");

            migrationBuilder.DropColumn(
                name: "ReportedByUserId",
                table: "PrintIncidents");

            migrationBuilder.DropColumn(
                name: "ResolvedByUserId",
                table: "PrintIncidents");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_ReportedBy",
                table: "PrintIncidents",
                column: "ReportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_ResolvedBy",
                table: "PrintIncidents",
                column: "ResolvedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_PrintIncidents_Users_ReportedBy",
                table: "PrintIncidents",
                column: "ReportedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PrintIncidents_Users_ResolvedBy",
                table: "PrintIncidents",
                column: "ResolvedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrintIncidents_Users_ReportedBy",
                table: "PrintIncidents");

            migrationBuilder.DropForeignKey(
                name: "FK_PrintIncidents_Users_ResolvedBy",
                table: "PrintIncidents");

            migrationBuilder.DropIndex(
                name: "IX_PrintIncidents_ReportedBy",
                table: "PrintIncidents");

            migrationBuilder.DropIndex(
                name: "IX_PrintIncidents_ResolvedBy",
                table: "PrintIncidents");

            migrationBuilder.AddColumn<int>(
                name: "ReportedByUserId",
                table: "PrintIncidents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResolvedByUserId",
                table: "PrintIncidents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_ReportedByUserId",
                table: "PrintIncidents",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintIncidents_ResolvedByUserId",
                table: "PrintIncidents",
                column: "ResolvedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrintIncidents_Users_ReportedByUserId",
                table: "PrintIncidents",
                column: "ReportedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrintIncidents_Users_ResolvedByUserId",
                table: "PrintIncidents",
                column: "ResolvedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
