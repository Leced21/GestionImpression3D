using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;

namespace TestProject
{
    public class PdfExportServiceTests
    {
        private readonly IPdfExportService _service = new PdfExportService();

        private static Piece CreatePiece(bool withFicheProduitFields) => new Piece
        {
            Id = 1,
            Nom = "Vase Spirale",
            Reference = "REF-001",
            Description = withFicheProduitFields ? "Un vase design aux lignes torsadées." : "",
            Statut = PieceStatus.Commercialisable,
            Categorie = "Décoration",
            Materiau = "PLA",
            PrixVente = 24.90m,
            Couleurs = withFicheProduitFields ? "Blanc, Noir" : null,
            CapaciteContenance = withFicheProduitFields ? "1.2 L" : null,
            NormesCertifications = withFicheProduitFields ? "CE" : null,
            InstructionsUtilisation = withFicheProduitFields ? "Nettoyer avec un chiffon sec." : null,
            PrecautionsUsage = withFicheProduitFields ? "Ne pas exposer à la chaleur directe." : null,
            PublicCible = withFicheProduitFields ? "Tous publics" : null,
            Conditionnement = withFicheProduitFields ? "Boîte individuelle" : null,
            DimensionsColis = withFicheProduitFields ? "20 x 20 x 25 cm" : null,
            PoidsColisKg = withFicheProduitFields ? 0.6m : null,
            MoqUnites = withFicheProduitFields ? 1 : null,
            DelaiLivraisonJours = withFicheProduitFields ? 5 : null,
            PointsForts = withFicheProduitFields ? "Design unique, fabrication à la demande." : null,
            Faq = withFicheProduitFields ? "Est-ce lavable ? Oui, à la main." : null,
            TarifsDegressifs = withFicheProduitFields ? "À partir de 10 unités : -10%" : null
        };

        private static STLMetadata CreateStlMetadata() => new STLMetadata
        {
            PieceId = 1,
            FileName = "vase.stl",
            Volume = 120.5m,
            SurfaceArea = 300.2m,
            BoundingBoxX = 80,
            BoundingBoxY = 80,
            BoundingBoxZ = 150,
            EstimatedWeight = 149.4m,
            EstimatedPrintTime = 320,
            TriangleCount = 5000,
            IsWatertight = true,
            AnalyzedAt = DateTime.UtcNow
        };

        [Fact]
        public async Task ExportFicheProduitPdfAsync_WithStlMetadataAndAllFields_GeneratesNonEmptyPdf()
        {
            var pdf = await _service.ExportFicheProduitPdfAsync(CreatePiece(withFicheProduitFields: true), CreateStlMetadata());

            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 0);
        }

        [Fact]
        public async Task ExportFicheProduitPdfAsync_WithoutStlMetadataOrOptionalFields_GeneratesNonEmptyPdf()
        {
            var pdf = await _service.ExportFicheProduitPdfAsync(CreatePiece(withFicheProduitFields: false), stlMetadata: null);

            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 0);
        }
    }
}
