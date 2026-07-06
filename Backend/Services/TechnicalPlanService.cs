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
        private readonly ISTLAnalyzerService _stlAnalyzerService;
        private readonly IWebHostEnvironment _env;
        // Deux types de couleur distincts pour la même teinte : QuestPDF.Infrastructure.Color
        // pour les éléments fluents (bordures/texte du document), SkiaSharp.SKColor pour le
        // dessin bas niveau (silhouette/cotations rastérisées en bitmap).
        private static readonly Color BrandNavy = Color.FromHex("#1B3A5C");
        private static readonly SKColor BrandNavySK = new SKColor(0x1B, 0x3A, 0x5C);

        public TechnicalPlanService(
            IServiceProvider serviceProvider,
            AppDbContext context,
            IPieceService pieceService,
            ISTLAnalyzerService stlAnalyzerService,
            IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _pieceService = pieceService;
            _stlAnalyzerService = stlAnalyzerService;
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private string? ResolveStlFilePath(string? stlFileName)
        {
            if (string.IsNullOrEmpty(stlFileName)
                || !string.Equals(Path.GetExtension(stlFileName), ".stl", StringComparison.OrdinalIgnoreCase))
                return null;

            var filePath = Path.Combine(_env.ContentRootPath, "uploads", stlFileName);
            return System.IO.File.Exists(filePath) ? filePath : null;
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
            var stlFilePath = ResolveStlFilePath(piece.StlFileName);

            if (metadata == null && stlFilePath != null)
            {
                metadata = await _pieceService.AnalyzeAndSaveStlFileAsync(pieceId, stlFilePath, piece.StlFileName);
            }

            if (metadata == null)
                throw new ArgumentException($"Pas de données STL pour la pièce {pieceId}");

            SilhouetteData? silhouette = null;
            byte[]? isoImage = null;
            if (stlFilePath != null)
            {
                using var stlStream = System.IO.File.OpenRead(stlFilePath);
                silhouette = await _stlAnalyzerService.ComputeSilhouetteAsync(stlStream);
                isoImage = await _stlAnalyzerService.GeneratePreviewAsync(stlStream);
            }

            return await Task.Run(() => GeneratePlanPdf(piece, metadata, silhouette, isoImage));
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

            // La génération du document QuestPDF est synchrone : on précalcule ici la
            // silhouette de chaque pièce (lecture de fichier + calcul asynchrones) pour
            // que GenerateProjectPlansPdf n'ait plus qu'à consulter ce dictionnaire.
            var silhouettesByPieceId = new Dictionary<int, SilhouetteData>();
            foreach (var projectPiece in project.ProjetPieces.Where(pp => pp.Piece != null))
            {
                var stlFilePath = ResolveStlFilePath(projectPiece.Piece.StlFileName);
                if (stlFilePath == null) continue;

                using var stlStream = System.IO.File.OpenRead(stlFilePath);
                silhouettesByPieceId[projectPiece.Piece.Id] = await _stlAnalyzerService.ComputeSilhouetteAsync(stlStream);
            }

            return await Task.Run(() => GenerateProjectPlansPdf(project, silhouettesByPieceId));
        }

        private byte[] GeneratePlanPdf(Piece piece, STLMetadata metadata, SilhouetteData? silhouette, byte[]? isoImage)
        {
            var maxDim = (float)Math.Max(metadata.BoundingBoxX, Math.Max(metadata.BoundingBoxY, metadata.BoundingBoxZ));
            const int cellSize = 230;
            var sharedScale = (cellSize - 50f) / Math.Max(maxDim, 0.1f);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.2f, Unit.Centimetre);

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

                    // Contenu : encadré façon feuille de dessin technique
                    page.Content()
                        .PaddingVertical(10)
                        .Border(1.2f).BorderColor(BrandNavy).Padding(12)
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

                            // Vues orthogonales (silhouette réelle projetée depuis le maillage STL,
                            // à une échelle commune aux 3 vues) + vue isométrique, disposition en
                            // grille façon feuille de dessin technique.
                            column.Item().PaddingTop(15)
                                .Column(inner =>
                                {
                                    inner.Item().PaddingBottom(10).Text($"VUES ORTHOGONALES — Échelle {FormatScale(sharedScale)}")
                                        .SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);

                                    inner.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().PaddingBottom(5).Text("Vue de face")
                                                        .SemiBold().FontSize(10);

                                                    RenderTechnicalView(
                                                        col.Item().Background(Colors.Grey.Lighten4).Padding(10),
                                                        silhouette?.Front ?? new List<SilhouetteEdge>(),
                                                        metadata.BoundingBoxX, metadata.BoundingBoxZ,
                                                        $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxZ:F1} mm",
                                                        $"{metadata.BoundingBoxY:F1} mm",
                                                        sharedScale, cellSize);
                                                });

                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().PaddingBottom(5).Text("Vue de côté")
                                                        .SemiBold().FontSize(10);

                                                    RenderTechnicalView(
                                                        col.Item().Background(Colors.Grey.Lighten4).Padding(10),
                                                        silhouette?.Side ?? new List<SilhouetteEdge>(),
                                                        metadata.BoundingBoxY, metadata.BoundingBoxZ,
                                                        $"{metadata.BoundingBoxY:F1} mm", $"{metadata.BoundingBoxZ:F1} mm",
                                                        $"{metadata.BoundingBoxX:F1} mm",
                                                        sharedScale, cellSize);
                                                });
                                        });

                                    inner.Item().PaddingTop(10)
                                        .Row(row =>
                                        {
                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().PaddingBottom(5).Text("Vue de dessus")
                                                        .SemiBold().FontSize(10);

                                                    RenderTechnicalView(
                                                        col.Item().Background(Colors.Grey.Lighten4).Padding(10),
                                                        silhouette?.Top ?? new List<SilhouetteEdge>(),
                                                        metadata.BoundingBoxX, metadata.BoundingBoxY,
                                                        $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxY:F1} mm",
                                                        $"{metadata.BoundingBoxZ:F1} mm",
                                                        sharedScale, cellSize);
                                                });

                                            row.RelativeColumn()
                                                .Column(col =>
                                                {
                                                    col.Item().PaddingBottom(5).Text("Vue isométrique")
                                                        .SemiBold().FontSize(10);

                                                    if (isoImage != null && isoImage.Length > 0)
                                                    {
                                                        col.Item().Background(Colors.Grey.Lighten4).Padding(10)
                                                            .Height(cellSize).Image(isoImage).FitArea();
                                                        col.Item().AlignCenter().PaddingTop(3)
                                                            .Text($"L {metadata.BoundingBoxX:F1} × l {metadata.BoundingBoxY:F1} × h {metadata.BoundingBoxZ:F1} mm")
                                                            .FontSize(7.5f).FontColor(Colors.Grey.Medium);
                                                    }
                                                    else
                                                    {
                                                        col.Item().Background(Colors.Grey.Lighten4).Padding(10)
                                                            .Height(cellSize).AlignCenter().AlignMiddle()
                                                            .Text("Non disponible").FontSize(9).FontColor(Colors.Grey.Medium);
                                                    }
                                                });
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

                            // Cartouche (bloc titre normalisé, en bas de la feuille de dessin)
                            column.Item().PaddingTop(15)
                                .Border(1).BorderColor(BrandNavy)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                    });

                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                        .Text("Désignation:").Bold().FontSize(8);
                                    table.Cell().Padding(5).Text(piece.Nom).FontSize(8);
                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                        .Text("Échelle:").Bold().FontSize(8);
                                    table.Cell().Padding(5).Text(FormatScale(sharedScale)).FontSize(8);

                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                        .Text("Référence:").Bold().FontSize(8);
                                    table.Cell().Padding(5).Text(piece.Reference).FontSize(8);
                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                        .Text("Matériau:").Bold().FontSize(8);
                                    table.Cell().Padding(5).Text(piece.Materiau ?? "N/A").FontSize(8);

                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                        .Text("Dessiné par:").Bold().FontSize(8);
                                    table.Cell().Padding(5).Text("3D Inspire (généré automatiquement)").FontSize(8);
                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                                        .Text("Date:").Bold().FontSize(8);
                                    table.Cell().Padding(5).Text($"{DateTime.Now:dd/MM/yyyy}").FontSize(8);

                                    table.Cell().ColumnSpan(4).Background(Colors.White).Padding(5)
                                        .Text("Sauf indication contraire : cotes en mm, tolérance générale ±0,2 mm. Lignes pointillées courtes = arêtes cachées, tireté-point = axes.")
                                        .FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                                });
                        });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateProjectPlansPdf(Projet project, Dictionary<int, SilhouetteData> silhouettesByPieceId)
        {
            var document = Document.Create(container =>
            {
                foreach (var projectPiece in project.ProjetPieces.Where(pp => pp.Piece != null))
                {
                    var metadata = _context.STLMetadata
                        .FirstOrDefault(m => m.PieceId == projectPiece.Piece.Id);

                    if (metadata == null) continue;

                    var piece = projectPiece.Piece;
                    silhouettesByPieceId.TryGetValue(piece.Id, out var silhouette);
                    var maxDim = (float)Math.Max(metadata.BoundingBoxX, Math.Max(metadata.BoundingBoxY, metadata.BoundingBoxZ));
                    const int cellSize = 145;
                    var sharedScale = (cellSize - 34f) / Math.Max(maxDim, 0.1f);

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
                                                            silhouette?.Front ?? new List<SilhouetteEdge>(),
                                                            metadata.BoundingBoxX, metadata.BoundingBoxZ,
                                                            $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxZ:F1} mm",
                                                            $"{metadata.BoundingBoxY:F1} mm",
                                                            sharedScale, cellSize);
                                                    });

                                                row.RelativeColumn()
                                                    .Column(col =>
                                                    {
                                                        col.Item().PaddingBottom(3).Text("Côté")
                                                            .SemiBold().FontSize(9);
                                                        RenderTechnicalView(
                                                            col.Item().Background(Colors.Grey.Lighten4).Padding(8),
                                                            silhouette?.Side ?? new List<SilhouetteEdge>(),
                                                            metadata.BoundingBoxY, metadata.BoundingBoxZ,
                                                            $"{metadata.BoundingBoxY:F1} mm", $"{metadata.BoundingBoxZ:F1} mm",
                                                            $"{metadata.BoundingBoxX:F1} mm",
                                                            sharedScale, cellSize);
                                                    });

                                                row.RelativeColumn()
                                                    .Column(col =>
                                                    {
                                                        col.Item().PaddingBottom(3).Text("Dessus")
                                                            .SemiBold().FontSize(9);
                                                        RenderTechnicalView(
                                                            col.Item().Background(Colors.Grey.Lighten4).Padding(8),
                                                            silhouette?.Top ?? new List<SilhouetteEdge>(),
                                                            metadata.BoundingBoxX, metadata.BoundingBoxY,
                                                            $"{metadata.BoundingBoxX:F1} mm", $"{metadata.BoundingBoxY:F1} mm",
                                                            $"{metadata.BoundingBoxZ:F1} mm",
                                                            sharedScale, cellSize);
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

        // Dessine une vue technique cotée : silhouette réelle projetée depuis le maillage STL
        // (contour + arêtes vives détectées façon logiciel de CAO, et non plus un simple
        // rectangle), lignes d'axe, et lignes de cote avec flèches et valeurs. Toutes les vues
        // d'une même feuille partagent la même échelle (points/mm), comme sur un vrai plan.
        // QuestPDF a retiré son API Canvas() (dépréciée depuis 2024.3.0, dépendance SkiaSharp
        // interne supprimée) : on rasterise donc nous-mêmes en bitmap puis on intègre le
        // résultat comme une image classique.
        private void RenderTechnicalView(IContainer container, List<SilhouetteEdge> edges, decimal widthMm, decimal heightMm, string widthLabel, string heightLabel, string depthLabel, float scale, int cellSize)
        {
            var realWidth = Math.Max((float)widthMm, 0.1f);
            var realHeight = Math.Max((float)heightMm, 0.1f);

            var pngBytes = RenderTechnicalViewToPng(cellSize, cellSize, realWidth, realHeight, edges, widthLabel, heightLabel, depthLabel, scale);
            container.Height(cellSize).Image(pngBytes).FitArea();
        }

        private static byte[] RenderTechnicalViewToPng(int canvasWidth, int canvasHeight, float realWidth, float realHeight, List<SilhouetteEdge> edges, string widthLabel, string heightLabel, string depthLabel, float scale)
        {
            // Rendu à résolution supérieure (canvas.Scale) puis intégré à la taille finale :
            // rend les traits/texte nets une fois affichés dans le PDF, au lieu du rendu un
            // peu pixelisé d'un bitmap 1 pixel = 1 point (~72 DPI).
            const int supersample = 3;
            using var bitmap = new SKBitmap(canvasWidth * supersample, canvasHeight * supersample);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                canvas.Scale(supersample);
                DrawTechnicalView(canvas, canvasWidth, canvasHeight, realWidth, realHeight, edges, widthLabel, heightLabel, depthLabel, scale);
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static void DrawTechnicalView(SKCanvas canvas, float canvasWidth, float canvasHeight, float realWidth, float realHeight, List<SilhouetteEdge> edges, string widthLabel, string heightLabel, string depthLabel, float scale)
        {
            const float marginLeft = 20f;
            const float marginBottom = 20f;

            var w = realWidth * scale;
            var h = realHeight * scale;

            var left = marginLeft + Math.Max((canvasWidth - marginLeft - w) / 2f, 0f);
            var top = Math.Max((canvasHeight - marginBottom - h) / 2f, 0f);
            var right = left + w;
            var bottom = top + h;

            using var objectPaint = new SKPaint
            {
                Color = BrandNavySK,
                StrokeWidth = 1.3f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };
            using var dashEffect = SKPathEffect.CreateDash(new float[] { 8f, 3f, 2f, 3f }, 0);
            using var centerPaint = new SKPaint
            {
                Color = BrandNavySK,
                StrokeWidth = 0.5f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                PathEffect = dashEffect
            };
            // Lignes cachées (arêtes vives du côté opposé à l'observateur) : pointillés
            // courts et réguliers, à distinguer du tireté-point des lignes d'axe.
            using var hiddenDashEffect = SKPathEffect.CreateDash(new float[] { 4f, 2.5f }, 0);
            using var hiddenPaint = new SKPaint
            {
                Color = BrandNavySK,
                StrokeWidth = 1f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                PathEffect = hiddenDashEffect
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

            if (edges.Count > 0)
            {
                // Bornes réelles de la silhouette projetée (peuvent différer légèrement de la
                // boîte englobante globale selon l'axe de vue) : on centre le tracé dessus.
                var minU = float.MaxValue; var maxU = float.MinValue;
                var minV = float.MaxValue; var maxV = float.MinValue;
                foreach (var e in edges)
                {
                    minU = Math.Min(minU, Math.Min(e.X1, e.X2));
                    maxU = Math.Max(maxU, Math.Max(e.X1, e.X2));
                    minV = Math.Min(minV, Math.Min(e.Y1, e.Y2));
                    maxV = Math.Max(maxV, Math.Max(e.Y1, e.Y2));
                }
                var centerU = (minU + maxU) / 2f;
                var centerV = (minV + maxV) / 2f;
                var centerX = (left + right) / 2f;
                var centerY = (top + bottom) / 2f;

                SKPoint ToScreen(float u, float v) => new SKPoint(
                    centerX + (u - centerU) * scale,
                    centerY - (v - centerV) * scale
                );

                foreach (var e in edges)
                {
                    canvas.DrawLine(ToScreen(e.X1, e.Y1), ToScreen(e.X2, e.Y2), e.IsHidden ? hiddenPaint : objectPaint);
                }
            }
            else
            {
                // Repli si la géométrie n'a pas pu être calculée : rectangle simple, pour ne
                // jamais laisser la vue complètement vide.
                canvas.DrawRect(left, top, w, h, objectPaint);
            }

            // Lignes d'axe (centre géométrique de la vue)
            canvas.DrawLine(left - 6f, (top + bottom) / 2f, right + 6f, (top + bottom) / 2f, centerPaint);
            canvas.DrawLine((left + right) / 2f, top - 6f, (left + right) / 2f, bottom + 6f, centerPaint);

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

            // 3e cote (profondeur), non représentable graphiquement dans cette vue 2D :
            // notée en texte pour que les 3 dimensions de la pièce soient cotées quelque
            // part sur chaque vue, plutôt que seulement les 2 dimensions du plan affiché.
            using var depthTextPaint = new SKPaint
            {
                Color = new SKColor(0x30, 0x30, 0x30),
                IsAntialias = true,
                TextSize = 7.5f,
                TextAlign = SKTextAlign.Left
            };
            canvas.DrawText($"Prof. : {depthLabel}", 4f, 10f, depthTextPaint);
        }

        // "1:N" (réduction) ou "N:1" (agrandissement) à partir de l'échelle points/mm
        // utilisée pour tracer les vues, convertie en ratio taille-imprimée / taille-réelle
        // (1 point = 1/72 pouce = 0,3527 mm).
        private static string FormatScale(float pointsPerMm)
        {
            const float mmPerPoint = 25.4f / 72f;
            var printedMmPerRealMm = pointsPerMm * mmPerPoint;

            return printedMmPerRealMm >= 1f
                ? $"{printedMmPerRealMm:F1}:1"
                : $"1:{1f / printedMmPerRealMm:F1}";
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
