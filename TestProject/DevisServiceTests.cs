using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;

namespace TestProject
{
    public class DevisServiceTests
    {
        private readonly IDevisService _devisService;
        private readonly Mock<IDevisRepository> _devisRepositoryMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IPieceRepository> _pieceRepositoryMock;
        private readonly Mock<IOrdreFabricationService> _ordreFabricationServiceMock;
        private readonly Mock<IFactureService> _factureServiceMock;
        private readonly Mock<IPdfExportService> _pdfExportServiceMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;

        public DevisServiceTests()
        {
            _devisRepositoryMock = new Mock<IDevisRepository>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _pieceRepositoryMock = new Mock<IPieceRepository>();
            _ordreFabricationServiceMock = new Mock<IOrdreFabricationService>();
            _factureServiceMock = new Mock<IFactureService>();
            _pdfExportServiceMock = new Mock<IPdfExportService>();
            _auditLoggerMock = new Mock<IAuditLogger>();

            _devisService = new DevisService(
                _devisRepositoryMock.Object,
                _clientRepositoryMock.Object,
                _pieceRepositoryMock.Object,
                _ordreFabricationServiceMock.Object,
                _factureServiceMock.Object,
                _pdfExportServiceMock.Object,
                _auditLoggerMock.Object
            );
        }

        private static Devis CreateDevis(int id, DevisStatus statut, int? projetId, params DevisLigne[] lignes)
        {
            return new Devis
            {
                Id = id,
                NumeroDevis = $"DEV-2026-{id:D4}",
                ProjetId = projetId,
                Statut = statut,
                Lignes = lignes.ToList()
            };
        }

        [Fact]
        public async Task UpdateAsync_NonExistingDevis_ReturnsNull()
        {
            _devisRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Devis?)null);

            var result = await _devisService.UpdateAsync(99, new UpdateDevisRequest { ClientId = 1, DateValidite = DateTime.UtcNow });

            Assert.Null(result);
            _devisRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Devis>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_OnAcceptedDevis_ThrowsInvalidOperationException()
        {
            var devis = CreateDevis(1, DevisStatus.Accepté, projetId: 7);
            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _devisService.UpdateAsync(1, new UpdateDevisRequest { ClientId = 1, DateValidite = DateTime.UtcNow })
            );
            _devisRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Devis>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingClient_ThrowsInvalidOperationException()
        {
            var devis = CreateDevis(1, DevisStatus.Brouillon, projetId: null);
            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _devisService.UpdateAsync(1, new UpdateDevisRequest { ClientId = 99, DateValidite = DateTime.UtcNow })
            );
            _devisRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Devis>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_RecomputesTotalsAndReplacesLignes()
        {
            var devis = CreateDevis(1, DevisStatus.Brouillon, projetId: null,
                new DevisLigne { Id = 1, Description = "Ancienne ligne", Quantite = 1, PrixUnitaire = 999m });
            var client = new Client { Id = 1, Nom = "Test SARL", Email = "contact@testsarl.com" };

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(client);
            _devisRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Devis>())).ReturnsAsync((Devis d) => d);

            var request = new UpdateDevisRequest
            {
                ClientId = 1,
                ProjetId = 7,
                DateValidite = DateTime.UtcNow.AddDays(30),
                TVA = 20,
                Lignes = new List<DevisLigneRequest>
                {
                    new DevisLigneRequest { Description = "Nouvelle ligne", Quantite = 2, PrixUnitaire = 50m }
                }
            };

            var result = await _devisService.UpdateAsync(1, request);

            Assert.NotNull(result);
            Assert.Equal(7, result.ProjetId);
            Assert.Single(result.Lignes);
            Assert.Equal("Nouvelle ligne", result.Lignes[0].Description);
            Assert.Equal(100m, result.TotalHT);
            Assert.Equal(120m, result.TotalTTC);
            _auditLoggerMock.Verify(x => x.LogUpdateAsync(EntityType.Devis, 1, "Devis", "Modifié", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithPieceLigne_UsesPiecePriceAndName()
        {
            var devis = CreateDevis(1, DevisStatus.Brouillon, projetId: null);
            var client = new Client { Id = 1, Nom = "Test SARL", Email = "contact@testsarl.com" };
            var piece = new Piece { Id = 10, Nom = "Pièce Catalogue", PrixVente = 42m };

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(client);
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(piece);
            _devisRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Devis>())).ReturnsAsync((Devis d) => d);

            var request = new UpdateDevisRequest
            {
                ClientId = 1,
                DateValidite = DateTime.UtcNow.AddDays(30),
                TVA = 0,
                Lignes = new List<DevisLigneRequest>
                {
                    new DevisLigneRequest { PieceId = 10, Quantite = 1, PrixUnitaire = 0 }
                }
            };

            var result = await _devisService.UpdateAsync(1, request);

            Assert.NotNull(result);
            Assert.Equal("Pièce Catalogue", result.Lignes[0].Description);
            Assert.Equal(42m, result.Lignes[0].PrixUnitaire);
        }

        [Fact]
        public async Task UpdateStatut_ToAccepteWithProjetAndPieceLignes_GeneratesOneOrdrePerLigne()
        {
            // Arrange
            var lignes = new[]
            {
                new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 },
                new DevisLigne { Id = 2, PieceId = 20, Quantite = 5 }
            };
            var devis = CreateDevis(1, DevisStatus.Envoyé, projetId: 7, lignes);
            var accepted = CreateDevis(1, DevisStatus.Accepté, projetId: 7, lignes);

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _devisRepositoryMock.Setup(x => x.UpdateStatutAsync(1, DevisStatus.Accepté)).ReturnsAsync(accepted);
            _ordreFabricationServiceMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(false);

            // Act
            var result = await _devisService.UpdateStatutAsync(1, DevisStatus.Accepté);

            // Assert
            Assert.NotNull(result);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.Is<CreateOrdreRequest>(r =>
                r.ProjetId == 7 && r.PieceId == 10 && r.Quantite == 2 && r.DevisId == 1)), Times.Once);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.Is<CreateOrdreRequest>(r =>
                r.ProjetId == 7 && r.PieceId == 20 && r.Quantite == 5 && r.DevisId == 1)), Times.Once);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateOrdreRequest>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateStatut_ToAccepteWithoutProjet_ThrowsAndDoesNotPersistStatut()
        {
            // Arrange
            var devis = CreateDevis(1, DevisStatus.Envoyé, projetId: null);
            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _devisService.UpdateStatutAsync(1, DevisStatus.Accepté)
            );
            _devisRepositoryMock.Verify(x => x.UpdateStatutAsync(It.IsAny<int>(), It.IsAny<DevisStatus>()), Times.Never);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateOrdreRequest>()), Times.Never);
            _factureServiceMock.Verify(x => x.CreateFromDevisAsync(It.IsAny<Devis>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatut_ToAccepteWithProjet_GeneratesFacture()
        {
            // Arrange
            var lignes = new[] { new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 } };
            var devis = CreateDevis(1, DevisStatus.Envoyé, projetId: 7, lignes);
            var accepted = CreateDevis(1, DevisStatus.Accepté, projetId: 7, lignes);

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _devisRepositoryMock.Setup(x => x.UpdateStatutAsync(1, DevisStatus.Accepté)).ReturnsAsync(accepted);
            _ordreFabricationServiceMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(false);
            _factureServiceMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(false);

            // Act
            await _devisService.UpdateStatutAsync(1, DevisStatus.Accepté);

            // Assert
            _factureServiceMock.Verify(x => x.CreateFromDevisAsync(It.Is<Devis>(d => d.Id == 1)), Times.Once);
        }

        [Fact]
        public async Task UpdateStatut_ToAccepteWhenFactureAlreadyExistsForDevis_SkipsGeneration()
        {
            // Arrange : facture déjà générée précédemment (rejeu du statut Accepté)
            var lignes = new[] { new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 } };
            var devis = CreateDevis(1, DevisStatus.Envoyé, projetId: 7, lignes);
            var accepted = CreateDevis(1, DevisStatus.Accepté, projetId: 7, lignes);

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _devisRepositoryMock.Setup(x => x.UpdateStatutAsync(1, DevisStatus.Accepté)).ReturnsAsync(accepted);
            _ordreFabricationServiceMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(false);
            _factureServiceMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(true);

            // Act
            await _devisService.UpdateStatutAsync(1, DevisStatus.Accepté);

            // Assert
            _factureServiceMock.Verify(x => x.CreateFromDevisAsync(It.IsAny<Devis>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatut_ToAccepteAlreadyAccepted_DoesNotRegenerateOrdres()
        {
            // Arrange : le devis est déjà Accepté, on renvoie le même statut (idempotence au niveau transition)
            var devis = CreateDevis(1, DevisStatus.Accepté, projetId: 7,
                new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 });
            var updated = CreateDevis(1, DevisStatus.Accepté, projetId: 7,
                new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 });

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _devisRepositoryMock.Setup(x => x.UpdateStatutAsync(1, DevisStatus.Accepté)).ReturnsAsync(updated);

            // Act
            await _devisService.UpdateStatutAsync(1, DevisStatus.Accepté);

            // Assert
            _ordreFabricationServiceMock.Verify(x => x.ExistsForDevisAsync(It.IsAny<int>()), Times.Never);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateOrdreRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatut_ToAccepteWhenOrdresAlreadyExistForDevis_SkipsGeneration()
        {
            // Arrange : statut Accepté renvoyé une seconde fois par erreur (rejeu), des ordres existent déjà
            var lignes = new[] { new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 } };
            var devis = CreateDevis(1, DevisStatus.Envoyé, projetId: 7, lignes);
            var accepted = CreateDevis(1, DevisStatus.Accepté, projetId: 7, lignes);

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _devisRepositoryMock.Setup(x => x.UpdateStatutAsync(1, DevisStatus.Accepté)).ReturnsAsync(accepted);
            _ordreFabricationServiceMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(true);

            // Act
            await _devisService.UpdateStatutAsync(1, DevisStatus.Accepté);

            // Assert
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateOrdreRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatut_ToAccepteWithLignesWithoutPieceId_SkipsThoseLignes()
        {
            // Arrange : ligne de service libre (sans pièce catalogue) mélangée à une ligne catalogue
            var lignes = new[]
            {
                new DevisLigne { Id = 1, PieceId = null, Description = "Frais de dossier", Quantite = 1 },
                new DevisLigne { Id = 2, PieceId = 10, Quantite = 3 }
            };
            var devis = CreateDevis(1, DevisStatus.Envoyé, projetId: 7, lignes);
            var accepted = CreateDevis(1, DevisStatus.Accepté, projetId: 7, lignes);

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _devisRepositoryMock.Setup(x => x.UpdateStatutAsync(1, DevisStatus.Accepté)).ReturnsAsync(accepted);
            _ordreFabricationServiceMock.Setup(x => x.ExistsForDevisAsync(1)).ReturnsAsync(false);

            // Act
            await _devisService.UpdateStatutAsync(1, DevisStatus.Accepté);

            // Assert
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateOrdreRequest>()), Times.Once);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.Is<CreateOrdreRequest>(r => r.PieceId == 10)), Times.Once);
        }

        [Fact]
        public async Task UpdateStatut_ToNonAccepteStatut_DoesNotGenerateOrdres()
        {
            // Arrange
            var devis = CreateDevis(1, DevisStatus.Envoyé, projetId: 7,
                new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 });
            var refused = CreateDevis(1, DevisStatus.Refusé, projetId: 7,
                new DevisLigne { Id = 1, PieceId = 10, Quantite = 2 });

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _devisRepositoryMock.Setup(x => x.UpdateStatutAsync(1, DevisStatus.Refusé)).ReturnsAsync(refused);

            // Act
            var result = await _devisService.UpdateStatutAsync(1, DevisStatus.Refusé);

            // Assert
            Assert.NotNull(result);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateOrdreRequest>()), Times.Never);
            _factureServiceMock.Verify(x => x.CreateFromDevisAsync(It.IsAny<Devis>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatut_NonExistingDevis_ReturnsNull()
        {
            _devisRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Devis?)null);

            var result = await _devisService.UpdateStatutAsync(99, DevisStatus.Accepté);

            Assert.Null(result);
            _ordreFabricationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateOrdreRequest>()), Times.Never);
            _factureServiceMock.Verify(x => x.CreateFromDevisAsync(It.IsAny<Devis>()), Times.Never);
        }

        [Fact]
        public async Task GeneratePdf_ExistingDevis_DelegatesToExportService()
        {
            var devis = CreateDevis(1, DevisStatus.Brouillon, projetId: null);
            var pdfBytes = new byte[] { 1, 2, 3 };

            _devisRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);
            _pdfExportServiceMock.Setup(x => x.ExportDevisPdfAsync(devis)).ReturnsAsync(pdfBytes);

            var result = await _devisService.GeneratePdfAsync(1);

            Assert.Equal(pdfBytes, result);
        }

        [Fact]
        public async Task GeneratePdf_NonExistingDevis_ReturnsEmptyBytes()
        {
            _devisRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Devis?)null);

            var result = await _devisService.GeneratePdfAsync(99);

            Assert.Empty(result);
            _pdfExportServiceMock.Verify(x => x.ExportDevisPdfAsync(It.IsAny<Devis>()), Times.Never);
        }
    }
}
