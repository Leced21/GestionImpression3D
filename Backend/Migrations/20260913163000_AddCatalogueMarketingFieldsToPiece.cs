using Backend.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260913163000_AddCatalogueMarketingFieldsToPiece")]
    public partial class AddCatalogueMarketingFieldsToPiece : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("NomCommercial", "Pieces", "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>("SloganProduit", "Pieces", "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>("AccrocheMarketing", "Pieces", "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>("DescriptionMarketing", "Pieces", "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<decimal>("HauteurCm", "Pieces", "decimal(18,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>("OuvertureCm", "Pieces", "decimal(18,2)", nullable: true);
            migrationBuilder.AddColumn<string>("MatiereMarketing", "Pieces", "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<bool>("EstEtanche", "Pieces", "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>("UtilisationProduit", "Pieces", "nvarchar(300)", maxLength: 300, nullable: true);
            migrationBuilder.AddColumn<string>("ConseilsEntretien", "Pieces", "nvarchar(300)", maxLength: 300, nullable: true);
            migrationBuilder.AddColumn<string>("ColorisDisponibles", "Pieces", "nvarchar(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<string>("BeneficesMarketing", "Pieces", "nvarchar(1000)", maxLength: 1000, nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("NomCommercial", "Pieces");
            migrationBuilder.DropColumn("SloganProduit", "Pieces");
            migrationBuilder.DropColumn("AccrocheMarketing", "Pieces");
            migrationBuilder.DropColumn("DescriptionMarketing", "Pieces");
            migrationBuilder.DropColumn("HauteurCm", "Pieces");
            migrationBuilder.DropColumn("OuvertureCm", "Pieces");
            migrationBuilder.DropColumn("MatiereMarketing", "Pieces");
            migrationBuilder.DropColumn("EstEtanche", "Pieces");
            migrationBuilder.DropColumn("UtilisationProduit", "Pieces");
            migrationBuilder.DropColumn("ConseilsEntretien", "Pieces");
            migrationBuilder.DropColumn("ColorisDisponibles", "Pieces");
            migrationBuilder.DropColumn("BeneficesMarketing", "Pieces");
        }
    }
}
