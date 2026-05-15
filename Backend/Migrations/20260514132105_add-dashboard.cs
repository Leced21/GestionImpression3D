using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class adddashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalPieces = table.Column<int>(type: "int", nullable: false),
                    EnConception = table.Column<int>(type: "int", nullable: false),
                    EnPrototypage = table.Column<int>(type: "int", nullable: false),
                    EnProduction = table.Column<int>(type: "int", nullable: false),
                    Commercialisables = table.Column<int>(type: "int", nullable: false),
                    ChiffreAffaires = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardStats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardStats");
        }
    }
}
