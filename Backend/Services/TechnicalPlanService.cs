using Backend.Data;
using Backend.Interface;
using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

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
        private readonly IPieceService _pieceService;
        private readonly IWebHostEnvironment _env;

        public TechnicalPlanService(
            IServiceProvider serviceProvider,
            AppDbContext context,
            IPieceService pieceService,
            IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _pieceService = pieceService;
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Génère un PDF avec les plans techniques d'une pièce
        /// </summary>
        public async Task<byte[]> GenerateTechnicalPlanPdfAsync(int pieceId)
        {
            var piece = await _context.Pieces
                .FirstOrDefaultAsync(p => p.Id == pieceId);

            if (piece == null)
                throw new ArgumentException($"Pièce avec l'ID {pieceId} non trouvée");

            var metadata = await _context.STLMetadata
                .FirstOrDefaultAsync(m => m.PieceId == pieceId);

            // Pièces uploadées avant la mise en place de l'analyse automatique à l'upload :
            // pas d'échec pour autant, on analyse le fichier déjà présent sur le disque
            // (uniquement pour un vrai .stl : les .step/.3mf ne sont pas ce format).
            if (metadata == null
                && !string.IsNullOrEmpty(piece.StlFileName)
                && string.Equals(Path.GetExtension(piece.StlFileName), ".stl", StringComparison.OrdinalIgnoreCase))
            {
                var filePath = Path.Combine(_env.ContentRootPath, "uploads", piece.StlFileName);
                if (System.IO.File.Exists(filePath))
                {
                    metadata = await _pieceService.AnalyzeAndSaveStlFileAsync(pieceId, filePath, piece.StlFileName);
                }
            }

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
                                    inner.Item().PaddingBottom(5).Text("INFORMATIONS GÉNÉRALES")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);

                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                        });

                                        // Désignation / Nom
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text("Désignation:").Bold().FontSize(9);
                                        table.Cell().Padding(5).Text(piece.Nom).FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text("Catégorie:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text(piece.Categorie ?? "N/A").FontSize(9);

                                        // Matériau / Statut
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text("Matériau:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text(piece.Materiau ?? "N/A").FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text("Statut:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text(piece.Statut.ToString()).FontSize(9);
                                    });
                                });

                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().PaddingBottom(5).Text("DIMENSIONS (mm)")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);

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
                                    inner.Item().PaddingBottom(10).Text("VUES ORTHOGONALES")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);

                                    // Vue de face
                                    inner.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().PaddingBottom(5).Text("Vue de face (XY)")
                                                        .SemiBold().FontSize(10);

                                                    RenderTechnicalView(
                                                        col.Item().Background(Colors.Grey.Lighten4).Padding(10),
                                                        metadata.BoundingBoxX, metadata.BoundingBoxZ,
                                                        $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxZ:F1} mm");
                                                });

                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().PaddingBottom(5).Text("Vue de côté (XZ)")
                                                        .SemiBold().FontSize(10);

                                                    RenderTechnicalView(
                                                        col.Item().Background(Colors.Grey.Lighten4).Padding(10),
                                                        metadata.BoundingBoxY, metadata.BoundingBoxZ,
                                                        $"{metadata.BoundingBoxY:F1} mm", $"{metadata.BoundingBoxZ:F1} mm");
                                                });
                                        });

                                    // Vue du dessus
                                    inner.Item().PaddingTop(10)
                                        .Column(col =>
                                        {
                                            col.Item().PaddingBottom(5).Text("Vue du dessus (YZ)")
                                                .SemiBold().FontSize(10);

                                            RenderTechnicalView(
                                                col.Item().Background(Colors.Grey.Lighten4).Padding(10),
                                                metadata.BoundingBoxX, metadata.BoundingBoxY,
                                                $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxY:F1} mm");
                                        });
                                });

                            // Données de costing
                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().PaddingBottom(5).Text("COÛTS DE PRODUCTION")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);

                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                        });

                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text("Coût matière:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text($"{piece.CoutMatiere:F2} €").FontSize(9);

                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text("Coût machine:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text($"{piece.CoutMachine:F2} €").FontSize(9);

                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text("Coût main-d'œuvre:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text($"{piece.CoutMainOeuvre:F2} €").FontSize(9);

                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text("Coût total:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text($"{piece.CoutTotal:F2} €").FontSize(9);

                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text("Prix de vente:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text($"{piece.PrixVente:F2} €").FontSize(9);

                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text("Marge:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text($"{piece.Marge:F2} € ({piece.MargePourcentage:F1}%)").FontSize(9);
                                    });
                                });

                            // Informations supplémentaires
                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().PaddingBottom(5).Text("CARACTÉRISTIQUES STL")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);

                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                        });

                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text("Surface:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text($"{metadata.SurfaceArea:F2} cm²").FontSize(9);

                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text("Étanche:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text(metadata.IsWatertight ? "Oui" : "Non").FontSize(9);

                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text("Nb triangles:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                            .Text(metadata.TriangleCount.ToString()).FontSize(9);

                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text("Temps imp.:").Bold().FontSize(9);
                                        table.Cell().Background(Colors.White).Padding(5)
                                            .Text($"{metadata.EstimatedPrintTime:F0} min").FontSize(9);
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
                foreach (var projectPiece in project.ProjetPieces.Where(pp => pp.Piece != null))
                {
                    var metadata = _context.STLMetadata
                        .FirstOrDefault(m => m.PieceId == projectPiece.Piece.Id);

                    if (metadata == null) continue;

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
                                            .Text($"Pièce {project.ProjetPieces.IndexOf(projectPiece) + 1}/{project.ProjetPieces.Count}")
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
                                column.Item().PaddingBottom(10).Text(piece.Nom)
                                    .SemiBold().FontSize(14);

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
                                                        col.Item().PaddingBottom(3).Text("Face")
                                                            .SemiBold().FontSize(9);
                                                        RenderTechnicalView(
                                                            col.Item().Background(Colors.Grey.Lighten4).Padding(8),
                                                            metadata.BoundingBoxX, metadata.BoundingBoxZ,
                                                            $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxZ:F1} mm", 60f);
                                                    });

                                                row.RelativeColumn()
                                                    .Column(col =>
                                                    {
                                                        col.Item().PaddingBottom(3).Text("Côté")
                                                            .SemiBold().FontSize(9);
                                                        RenderTechnicalView(
                                                            col.Item().Background(Colors.Grey.Lighten4).Padding(8),
                                                            metadata.BoundingBoxY, metadata.BoundingBoxZ,
                                                            $"{metadata.BoundingBoxY:F1} mm", $"{metadata.BoundingBoxZ:F1} mm", 60f);
                                                    });

                                                row.RelativeColumn()
                                                    .Column(col =>
                                                    {
                                                        col.Item().PaddingBottom(3).Text("Dessus")
                                                            .SemiBold().FontSize(9);
                                                        RenderTechnicalView(
                                                            col.Item().Background(Colors.Grey.Lighten4).Padding(8),
                                                            metadata.BoundingBoxX, metadata.BoundingBoxY,
                                                            $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxY:F1} mm", 60f);
                                                    });
                                            });
                                    });
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text($"Page {project.ProjetPieces.IndexOf(projectPiece) + 1}/{project.ProjetPieces.Count} - {DateTime.Now:dd/MM/yyyy}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                }
            });

            return document.GeneratePdf();
        }

        // Dessine une vue technique cotée (silhouette à l'échelle + lignes de cote avec
        // flèches et valeurs) au lieu de l'ancien rendu ASCII, qui ne reflétait ni les
        // proportions ni la forme de la pièce et ne portait aucune cotation exploitable.
        private void RenderTechnicalView(IContainer container, decimal widthMm, decimal heightMm, string widthLabel, string heightLabel, float maxSize = 100f)
        {
            var realWidth = Math.Max((float)widthMm, 0.1f);
            var realHeight = Math.Max((float)heightMm, 0.1f);

            container.Height(maxSize + 32f).Canvas((canvasObj, size) =>
                DrawTechnicalView((SKCanvas)canvasObj, size, realWidth, realHeight, widthLabel, heightLabel));
        }

        private static void DrawTechnicalView(SKCanvas canvas, Size size, float realWidth, float realHeight, string widthLabel, string heightLabel)
        {
            const float marginLeft = 42f;
            const float marginBottom = 22f;
            const float marginTop = 8f;
            const float marginRight = 8f;

            var availableWidth = size.Width - marginLeft - marginRight;
            var availableHeight = size.Height - marginTop - marginBottom;
            if (availableWidth <= 0 || availableHeight <= 0) return;

            var scale = Math.Min(availableWidth / realWidth, availableHeight / realHeight);
            var w = realWidth * scale;
            var h = realHeight * scale;

            var left = marginLeft + (availableWidth - w) / 2f;
            var top = marginTop + (availableHeight - h) / 2f;
            var right = left + w;
            var bottom = top + h;

            using var outlinePaint = new SKPaint
            {
                Color = new SKColor(0x1B, 0x3A, 0x5C),
                StrokeWidth = 1.5f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            using var fillPaint = new SKPaint
            {
                Color = new SKColor(0xEA, 0xF0, 0xF6),
                Style = SKPaintStyle.Fill
            };
            using var dimPaint = new SKPaint
            {
                Color = new SKColor(0x60, 0x60, 0x60),
                StrokeWidth = 0.75f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            using var textPaint = new SKPaint
            {
                Color = new SKColor(0x30, 0x30, 0x30),
                IsAntialias = true,
                TextSize = 8.5f,
                TextAlign = SKTextAlign.Center
            };

            // Silhouette de la pièce, à l'échelle
            canvas.DrawRect(left, top, w, h, fillPaint);
            canvas.DrawRect(left, top, w, h, outlinePaint);

            // Cotation horizontale (largeur), sous la pièce : lignes d'attache + ligne de
            // cote avec flèches aux deux extrémités + valeur centrée.
            var dimY = bottom + 14f;
            canvas.DrawLine(left, bottom, left, dimY + 4f, dimPaint);
            canvas.DrawLine(right, bottom, right, dimY + 4f, dimPaint);
            canvas.DrawLine(left, dimY, right, dimY, dimPaint);
            DrawArrowhead(canvas, dimPaint.Color, left, dimY, 1f, 0f);
            DrawArrowhead(canvas, dimPaint.Color, right, dimY, -1f, 0f);
            canvas.DrawText(widthLabel, (left + right) / 2f, dimY - 4f, textPaint);

            // Cotation verticale (hauteur), à gauche de la pièce
            var dimX = left - 14f;
            canvas.DrawLine(left, top, dimX - 4f, top, dimPaint);
            canvas.DrawLine(left, bottom, dimX - 4f, bottom, dimPaint);
            canvas.DrawLine(dimX, top, dimX, bottom, dimPaint);
            DrawArrowhead(canvas, dimPaint.Color, dimX, top, 0f, 1f);
            DrawArrowhead(canvas, dimPaint.Color, dimX, bottom, 0f, -1f);

            canvas.Save();
            canvas.Translate(dimX - 4f, (top + bottom) / 2f);
            canvas.RotateDegrees(-90);
            canvas.DrawText(heightLabel, 0, 0, textPaint);
            canvas.Restore();
        }

        // Triangle plein dont la pointe est en (tipX, tipY) et dont la base s'évase dans la
        // direction (dirX, dirY) : matérialise l'extrémité d'une ligne de cote (norme dessin
        // technique), la pointe touchant exactement le point mesuré.
        private static void DrawArrowhead(SKCanvas canvas, SKColor color, float tipX, float tipY, float dirX, float dirY)
        {
            const float length = 5f;
            const float halfWidth = 1.8f;
            var perpX = -dirY;
            var perpY = dirX;

            using var path = new SKPath();
            path.MoveTo(tipX, tipY);
            path.LineTo(tipX + dirX * length + perpX * halfWidth, tipY + dirY * length + perpY * halfWidth);
            path.LineTo(tipX + dirX * length - perpX * halfWidth, tipY + dirY * length - perpY * halfWidth);
            path.Close();

            using var fillPaint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawPath(path, fillPaint);
        }

        private void CreateTableCell(TableDescriptor table, string label, string value, bool isEven)
        {
            var backgroundColor = isEven ? Colors.Grey.Lighten4 : Colors.White;

            table.Cell().Background(backgroundColor).Padding(5)
                .Text(label).Bold().FontSize(9);
            table.Cell().Background(backgroundColor).Padding(5)
                .Text(value).FontSize(9);
        }
    }
}
