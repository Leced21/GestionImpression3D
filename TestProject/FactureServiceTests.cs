using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;

namespace TestProject
{
    public class FactureServiceTests
    {
        private readonly IFactureService _factureService;
        private readonly Mock<IFactureRepository> _factureRepositoryMock;
        private readonly Mock<IPdfExportService> _pdfExportServiceMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;

        public FactureServiceTests()
        {
            _factureRepositoryMock = new Mock<IFactureRepository>();
            _pdfExportServiceMock = new Mock<IPdfExportService>();
            _auditLoggerMock = new Mock<IAuditLogger>();

            _factureService = new FactureService(_factureRepositoryMock.Object, _pdfExportServiceMock.Object, _auditLoggerMock.Object);
        }

        private static Devis CreateAcceptedDevis()
        {
            return new Devis
            {
                Id = 1,
                NumeroDevis = "DEV-2026-0001",
                ClientId = 7,
                TotalHT = 100m,
                TVA = 20m,
                TotalTTC = 120m,
                Statut = DevisStatus.Accepté,
                Lignes = new List<DevisLigne>
                {
                    new DevisLigne { Id = 1, PieceId = 10, Description = "Piece A", Quantite = 2, PrixUnitaire = 50m }
                }
            };
        }

        [Fact]
        public async Task CreateFromDevis_BuildsFactureWithLignesAndSnapshotTotals()
        {
            // Arrange
            var devis = CreateAcceptedDevis();
            _factureRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Facture>())).ReturnsAsync((Facture f) => f);

            // Act
            var result = await _factureService.CreateFromDevisAsync(devis);

            // Assert
            Assert.Equal(1, result.DevisId);
            Assert.Equal(7, result.ClientId);
            Assert.Equal(100m, result.TotalHT);
            Assert.Equal(120m, result.TotalTTC);
            Assert.Equal(FactureStatus.Émise, result.Statut);
            Assert.Single(result.Lignes);
            Assert.Equal(10, result.Lignes[0].PieceId);
            Assert.Equal(2, result.Lignes[0].Quantite);
            _auditLoggerMock.Verify(x => x.LogCreationAsync(EntityType.Facture, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateFromDevis_SetsDueDateThirtyDaysAfterEmission()
        {
            // Arrange
            var devis = CreateAcceptedDevis();
            _factureRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Facture>())).ReturnsAsync((Facture f) => f);

            // Act
            var result = await _factureService.CreateFromDevisAsync(devis);

            // Assert
            Assert.True((result.DateEcheance - result.DateEmission).TotalDays is >= 29.9 and <= 30.1);
        }

        [Fact]
        public async Task ExistsForDevis_DelegatesToRepository()
        {
            _factureRepositoryMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(true);

            var result = await _factureService.ExistsForDevisAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateStatut_ExistingFacture_UpdatesStatusAndLogsChange()
        {
            // Arrange
            var facture = new Facture { Id = 1, NumeroFacture = "FACT-2026-0001", Statut = FactureStatus.Émise };
            _factureRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(facture);
            _factureRepositoryMock.Setup(x => x.UpdateStatutAsync(1, FactureStatus.Payée))
                .ReturnsAsync(new Facture { Id = 1, NumeroFacture = "FACT-2026-0001", Statut = FactureStatus.Payée });

            // Act
            var result = await _factureService.UpdateStatutAsync(1, FactureStatus.Payée);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(FactureStatus.Payée, result.Statut);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(EntityType.Facture, 1, "Émise", "Payée"), Times.Once);
        }

        [Fact]
        public async Task UpdateStatut_NonExistingFacture_ReturnsNull()
        {
            _factureRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Facture?)null);

            var result = await _factureService.UpdateStatutAsync(99, FactureStatus.Payée);

            Assert.Null(result);
            _factureRepositoryMock.Verify(x => x.UpdateStatutAsync(It.IsAny<int>(), It.IsAny<FactureStatus>()), Times.Never);
        }

        [Fact]
        public async Task GetByClient_DelegatesToRepository()
        {
            var factures = new List<Facture> { new Facture { Id = 1, ClientId = 7 } };
            _factureRepositoryMock.Setup(x => x.GetByClientAsync(7)).ReturnsAsync(factures);

            var result = await _factureService.GetByClientAsync(7);

            Assert.Single(result);
        }

        [Fact]
        public async Task GeneratePdf_NonExistingFacture_ReturnsEmptyBytes()
        {
            _factureRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Facture?)null);

            var result = await _factureService.GeneratePdfAsync(99);

            Assert.Empty(result);
            _pdfExportServiceMock.Verify(x => x.ExportFacturePdfAsync(It.IsAny<Facture>()), Times.Never);
        }

        [Fact]
        public async Task GeneratePdf_ExistingFacture_DelegatesToExportService()
        {
            var facture = new Facture { Id = 1, NumeroFacture = "FACT-2026-0001" };
            var pdfBytes = new byte[] { 4, 5, 6 };

            _factureRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(facture);
            _pdfExportServiceMock.Setup(x => x.ExportFacturePdfAsync(facture)).ReturnsAsync(pdfBytes);

            var result = await _factureService.GeneratePdfAsync(1);

            Assert.Equal(pdfBytes, result);
        }
    }
}
