using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConvertCommandeStatutToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Traduit les valeurs texte existantes vers le code entier de l'enum
            // Backend.Enums.CommandeStatus AVANT de changer le type de colonne, pour ne
            // perdre aucune donnée déjà en base.
            migrationBuilder.Sql(@"
                UPDATE [Commandes] SET [Statut] = CASE [Statut]
                    WHEN N'En attente' THEN N'1'
                    WHEN N'Confirmée' THEN N'2'
                    WHEN N'En production' THEN N'3'
                    WHEN N'Expédiée' THEN N'4'
                    WHEN N'Livrée' THEN N'5'
                    WHEN N'Annulée' THEN N'6'
                    ELSE N'1'
                END
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Statut",
                table: "Commandes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Commandes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"
                UPDATE [Commandes] SET [Statut] = CASE [Statut]
                    WHEN N'1' THEN N'En attente'
                    WHEN N'2' THEN N'Confirmée'
                    WHEN N'3' THEN N'En production'
                    WHEN N'4' THEN N'Expédiée'
                    WHEN N'5' THEN N'Livrée'
                    WHEN N'6' THEN N'Annulée'
                    ELSE N'En attente'
                END
            ");
        }
    }
}
