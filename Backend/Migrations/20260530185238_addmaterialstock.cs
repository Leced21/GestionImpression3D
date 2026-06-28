using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    MinThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supplier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastRestockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialStocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialStocks_IsActive",
                table: "MaterialStocks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialStocks_Quantity",
                table: "MaterialStocks",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialStocks_Type",
                table: "MaterialStocks",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialStocks");
        }
    }
}
