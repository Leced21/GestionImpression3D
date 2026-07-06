using Backend.Data;
using Backend.Interface;
using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    /// <summary>
    /// Service pour générer des plans techniques 2D des modèles STL
    /// avec vues orthogonales et cotations
    /// </summary>
    public class TechnicalPlanService : ITechnicalPlanService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _context;

        public TechnicalPlanService(IServiceProvider serviceProvider, AppDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Génère un PDF avec les plans techniques d'une pièce
        /// </summary>
        public async Task<byte[]> GenerateTechnicalPlanPdfAsync(int pieceId)
        {
            var piece = await _context.Pieces
                .Include(p => p.PieceVersions)
                .FirstOrDefaultAsync(p => p.Id == pieceId);

            if (piece == null)
                throw new ArgumentException($"Pièce avec l'ID {pieceId} non trouvée");

            var metadata = await _context.STLMetadata
                .FirstOrDefaultAsync(m => m.PieceId == pieceId);

            if (metadata == null)
                throw new ArgumentException($"Pas de données STL pour la pièce {pieceId}");

            return await Task.Run(() => GeneratePlanPdf(piece, metadata));
        }

        /// <summary>
        /// Génère un PDF avec les plans pour un projet entier
        /// </summary>
        public async Task<byte[]> GenerateProjectTechnicalPlansPdfAsync(int projectId)
        {
            var project = await _context.Projets
                .Include(p => p.ProjetPieces)
                .ThenInclude(pp => pp.Piece)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new ArgumentException($"Projet avec l'ID {projectId} non trouvé");

            return await Task.Run(() => GenerateProjectPlansPdf(project));
        }

        private byte[] GeneratePlanPdf(Piece piece, STLMetadata metadata)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);

                    // En-tête
                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeColumn().AlignLeft()
                                        .Text("PLAN TECHNIQUE")
                                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                                    row.RelativeColumn().AlignRight()
                                        .Text($"Réf. {piece.Reference}")
                                        .FontSize(10).FontColor(Colors.Grey.Medium);
                                });

                            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(5);
                        });

                    // Contenu
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            // Section Informations générales
                            column.Item()
                                .Column(inner =>
                                {
                                    inner.Item().Text("INFORMATIONS GÉNÉRALES")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium)
                                        .PaddingBottom(5);

                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                        });

                                        CreateTableCell(table, "Désignation:", piece.Nom, true);
                                        CreateTableCell(table, "Catégorie:", piece.Categorie ?? "N/A", false);

                                        CreateTableCell(table, "Matériau:", piece.Materiau ?? "N/A", true);
                                        CreateTableCell(table, "Statut:", piece.Statut.ToString(), false);
                                    });
                                });

                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().Text("DIMENSIONS (mm)")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium)
                                        .PaddingBottom(5);

                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                        });

                                        // En-têtes
                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                                .Text("Longueur (X)").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                                .Text("Largeur (Y)").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                                .Text("Hauteur (Z)").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                                .Text("Volume (cm³)").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                                .Text("Poids (g)").Bold();
                                        });

                                        // Données
                                        table.Cell().Padding(5).AlignCenter()
                                            .Text($"{metadata.BoundingBoxX:F2}");
                                        table.Cell().Padding(5).AlignCenter()
                                            .Text($"{metadata.BoundingBoxY:F2}");
                                        table.Cell().Padding(5).AlignCenter()
                                            .Text($"{metadata.BoundingBoxZ:F2}");
                                        table.Cell().Padding(5).AlignCenter()
                                            .Text($"{metadata.Volume:F2}");
                                        table.Cell().Padding(5).AlignCenter()
                                            .Text($"{metadata.EstimatedWeight:F2}");
                                    });
                                });

                            // Représentation ASCII des vues
                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().Text("VUES ORTHOGONALES")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium)
                                        .PaddingBottom(10);

                                    // Vue de face
                                    inner.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().Text("Vue de face (XY)")
                                                        .SemiBold().FontSize(10)
                                                        .PaddingBottom(5);

                                                    col.Item().Background(Colors.Grey.Lighten4)
                                                        .Padding(10)
                                                        .AlignCenter()
                                                        .Text(GenerateFaceViewAscii(metadata))
                                                        .FontFamily("Courier New")
                                                        .FontSize(8);
                                                });

                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().Text("Vue de côté (XZ)")
                                                        .SemiBold().FontSize(10)
                                                        .PaddingBottom(5);

                                                    col.Item().Background(Colors.Grey.Lighten4)
                                                        .Padding(10)
                                                        .AlignCenter()
                                                        .Text(GenerateSideViewAscii(metadata))
                                                        .FontFamily("Courier New")
                                                        .FontSize(8);
                                                });
                                        });

                                    // Vue du dessus
                                    inner.Item().PaddingTop(10)
                                        .Column(col =>
                                        {
                                            col.Item().Text("Vue du dessus (YZ)")
                                                .SemiBold().FontSize(10)
                                                .PaddingBottom(5);

                                            col.Item().Background(Colors.Grey.Lighten4)
                                                .Padding(10)
                                                .AlignCenter()
                                                .Text(GenerateTopViewAscii(metadata))
                                                .FontFamily("Courier New")
                                                .FontSize(8);
                                        });
                                });

                            // Données de costing
                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().Text("COÛTS DE PRODUCTION")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium)
                                        .PaddingBottom(5);

                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                        });

                                        CreateTableCell(table, "Coût matière:", $"{piece.CoutMatiere:F2} €", true);
                                        CreateTableCell(table, "Coût machine:", $"{piece.CoutMachine:F2} €", false);
                                        CreateTableCell(table, "Coût main-d'œuvre:", $"{piece.CoutMainOeuvre:F2} €", true);
                                        CreateTableCell(table, "Coût total:", $"{piece.CoutTotal:F2} €", false);
                                        CreateTableCell(table, "Prix de vente:", $"{piece.PrixVente:F2} €", true);
                                        CreateTableCell(table, "Marge:", $"{piece.Marge:F2} € ({piece.MargePourcentage:F1}%)", false);
                                    });
                                });

                            // Informations supplémentaires
                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().Text("CARACTÉRISTIQUES STL")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium)
                                        .PaddingBottom(5);

                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                        });

                                        CreateTableCell(table, "Surface:", $"{metadata.SurfaceArea:F2} mm²", true);
                                        CreateTableCell(table, "Étanche:", metadata.IsWatertight ? "Oui" : "Non", false);
                                        CreateTableCell(table, "Nb triangles:", metadata.TriangleCount.ToString(), true);
                                        CreateTableCell(table, "Temps imp.:", $"{metadata.EstimatedPrintTime:F0} min", false);
                                    });
                                });
                        });

                    // Pied de page
                    page.Footer()
                        .AlignCenter()
                        .Column(col =>
                        {
                            col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingTop(5)
                                .Text($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm} - 3D Inspire")
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateProjectPlansPdf(Projet project)
        {
            var document = Document.Create(container =>
            {
                var firstPage = true;

                foreach (var projectPiece in project.ProjetPieces.Where(pp => pp.Piece != null))
                {
                    var metadata = _context.STLMetadata
                        .FirstOrDefault(m => m.PieceId == projectPiece.Piece.Id);

                    if (metadata == null) continue;

                    if (!firstPage)
                    {
                        container.Page(page => page.PageBreak());
                    }

                    var piece = projectPiece.Piece;

                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.5f, Unit.Centimetre);

                        page.Header()
                            .Column(column =>
                            {
                                column.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeColumn().AlignLeft()
                                            .Text($"PROJET: {project.Reference}")
                                            .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);

                                        row.RelativeColumn().AlignRight()
                                            .Text($"Pièce {projectPiece.OrdreAffichage}/{project.ProjetPieces.Count}")
                                            .FontSize(10).FontColor(Colors.Grey.Medium);
                                    });

                                column.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Text($"Référence: {piece.Reference}")
                                            .FontSize(10);

                                        row.RelativeColumn()
                                            .AlignRight()
                                            .Text($"Quantité: {projectPiece.Quantite}")
                                            .FontSize(10);
                                    });

                                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .PaddingVertical(5);
                            });

                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Item().Text(piece.Nom)
                                    .SemiBold().FontSize(14)
                                    .PaddingBottom(10);

                                // Informations principales
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                    });

                                    CreateTableCell(table, "Matériau:", piece.Materiau ?? "N/A", true);
                                    CreateTableCell(table, "Catégorie:", piece.Categorie ?? "N/A", false);
                                    CreateTableCell(table, "Longueur (X):", $"{metadata.BoundingBoxX:F1} mm", true);
                                    CreateTableCell(table, "Largeur (Y):", $"{metadata.BoundingBoxY:F1} mm", false);
                                });

                                // Vues
                                column.Item().PaddingTop(15)
                                    .Column(inner =>
                                    {
                                        inner.Item()
                                            .Row(row =>
                                            {
                                                row.RelativeColumn()
                                                    .Column(col =>
                                                    {
                                                        col.Item().Text("Face")
                                                            .SemiBold().FontSize(9)
                                                            .PaddingBottom(3);
                                                        col.Item().Background(Colors.Grey.Lighten4)
                                                            .Padding(8)
                                                            .AlignCenter()
                                                            .Text(GenerateFaceViewAscii(metadata))
                                                            .FontFamily("Courier New")
                                                            .FontSize(7);
                                                    });

                                                row.RelativeColumn()
                                                    .Column(col =>
                                                    {
                                                        col.Item().Text("Côté")
                                                            .SemiBold().FontSize(9)
                                                            .PaddingBottom(3);
                                                        col.Item().Background(Colors.Grey.Lighten4)
                                                            .Padding(8)
                                                            .AlignCenter()
                                                            .Text(GenerateSideViewAscii(metadata))
                                                            .FontFamily("Courier New")
                                                            .FontSize(7);
                                                    });

                                                row.RelativeColumn()
                                                    .Column(col =>
                                                    {
                                                        col.Item().Text("Dessus")
                                                            .SemiBold().FontSize(9)
                                                            .PaddingBottom(3);
                                                        col.Item().Background(Colors.Grey.Lighten4)
                                                            .Padding(8)
                                                            .AlignCenter()
                                                            .Text(GenerateTopViewAscii(metadata))
                                                            .FontFamily("Courier New")
                                                            .FontSize(7);
                                                    });
                                            });
                                    });
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text($"Page {project.ProjetPieces.IndexOf(projectPiece) + 1}/{project.ProjetPieces.Count} - {DateTime.Now:dd/MM/yyyy}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                    firstPage = false;
                }
            });

            return document.GeneratePdf();
        }

        private string GenerateFaceViewAscii(STLMetadata metadata)
        {
            var width = (int)(metadata.BoundingBoxX / 10);
            var height = (int)(metadata.BoundingBoxZ / 10);

            width = Math.Max(width, 10);
            height = Math.Max(height, 8);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("┌" + new string('─', width) + "┐");

            for (int i = 0; i < height; i++)
            {
                if (i == height / 2)
                    sb.AppendLine("│" + CenterText("█████", width) + "│");
                else
                    sb.AppendLine("│" + new string(' ', width) + "│");
            }

            sb.AppendLine("└" + new string('─', width) + "┘");
            sb.Append($"  {metadata.BoundingBoxX:F1} mm");

            return sb.ToString();
        }

        private string GenerateSideViewAscii(STLMetadata metadata)
        {
            var width = (int)(metadata.BoundingBoxY / 10);
            var height = (int)(metadata.BoundingBoxZ / 10);

            width = Math.Max(width, 10);
            height = Math.Max(height, 8);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("┌" + new string('─', width) + "┐");

            for (int i = 0; i < height; i++)
            {
                if (i > height / 3 && i < 2 * height / 3)
                    sb.AppendLine("│" + CenterText("███", width) + "│");
                else
                    sb.AppendLine("│" + new string(' ', width) + "│");
            }

            sb.AppendLine("└" + new string('─', width) + "┘");
            sb.Append($"  {metadata.BoundingBoxY:F1} mm");

            return sb.ToString();
        }

        private string GenerateTopViewAscii(STLMetadata metadata)
        {
            var width = (int)(metadata.BoundingBoxX / 10);
            var height = (int)(metadata.BoundingBoxY / 10);

            width = Math.Max(width, 10);
            height = Math.Max(height, 8);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("┌" + new string('─', width) + "┐");

            for (int i = 0; i < height; i++)
            {
                if (i == 0 || i == height - 1)
                    sb.AppendLine("│" + new string('█', width) + "│");
                else if (i == height / 2)
                    sb.AppendLine("│" + CenterText("▓▓▓", width) + "│");
                else
                    sb.AppendLine("│" + new string(' ', width) + "│");
            }

            sb.AppendLine("└" + new string('─', width) + "┘");

            return sb.ToString();
        }

        private string CenterText(string text, int width)
        {
            var padding = (width - text.Length) / 2;
            return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
        }

        private void CreateTableCell(ITableDescriptor table, string label, string value, bool isEven)
        {
            var backgroundColor = isEven ? Colors.Grey.Lighten4 : Colors.White;

            table.Cell().Background(backgroundColor).Padding(5)
                .Text(label).Bold().FontSize(9);
            table.Cell().Background(backgroundColor).Padding(5)
                .Text(value).FontSize(9);
        }
    }
}
