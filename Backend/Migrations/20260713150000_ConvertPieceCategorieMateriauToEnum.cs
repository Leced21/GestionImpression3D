using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPieceCategorieMateriauToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Traduit les valeurs texte existantes (libellés français accentués) vers le nom
            // de l'enum Backend.Enums.PieceCategorie/PieceMateriau AVANT de rendre la colonne
            // obligatoire, pour ne perdre aucune donnée déjà en base (y compris les NULL, qui
            // tombent dans ELSE faute de correspondance avec les WHEN).
            migrationBuilder.Sql(@"
                UPDATE [Pieces] SET [Categorie] = CASE [Categorie]
                    WHEN N'Mécanique' THEN N'Mecanique'
                    WHEN N'Électronique' THEN N'Electronique'
                    WHEN N'Décoration' THEN N'Decoration'
                    WHEN N'Outillage' THEN N'Outillage'
                    ELSE N'Mecanique'
                END
            ");

            migrationBuilder.Sql(@"
                UPDATE [Pieces] SET [Materiau] = CASE [Materiau]
                    WHEN N'PLA' THEN N'PLA'
                    WHEN N'PETG' THEN N'PETG'
                    WHEN N'ABS' THEN N'ABS'
                    WHEN N'Résine' THEN N'Resine'
                    ELSE N'PLA'
                END
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Categorie",
                table: "Pieces",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Mecanique",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Materiau",
                table: "Pieces",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PLA",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Categorie",
                table: "Pieces",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Materiau",
                table: "Pieces",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.Sql(@"
                UPDATE [Pieces] SET [Categorie] = CASE [Categorie]
                    WHEN N'Mecanique' THEN N'Mécanique'
                    WHEN N'Electronique' THEN N'Électronique'
                    WHEN N'Decoration' THEN N'Décoration'
                    WHEN N'Outillage' THEN N'Outillage'
                    ELSE N'Mécanique'
                END
            ");

            migrationBuilder.Sql(@"
                UPDATE [Pieces] SET [Materiau] = CASE [Materiau]
                    WHEN N'PLA' THEN N'PLA'
                    WHEN N'PETG' THEN N'PETG'
                    WHEN N'ABS' THEN N'ABS'
                    WHEN N'Resine' THEN N'Résine'
                    ELSE N'PLA'
                END
            ");
        }
    }
}
