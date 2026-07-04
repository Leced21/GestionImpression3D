using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject
{
    public class OrdreFabricationServiceTests
    {
        private readonly IOrdreFabricationService _ordreService;
        private readonly Mock<IOrdreFabricationRepository> _ordreRepositoryMock;
        private readonly Mock<IProjetRepository> _projetRepositoryMock;
        private readonly Mock<IPieceRepository> _pieceRepositoryMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;

        public OrdreFabricationServiceTests()
        {
            _ordreRepositoryMock = new Mock<IOrdreFabricationRepository>();
            _projetRepositoryMock = new Mock<IProjetRepository>();
            _pieceRepositoryMock = new Mock<IPieceRepository>();
            _auditLoggerMock = new Mock<IAuditLogger>();

            _ordreService = new OrdreFabricationService(
                _ordreRepositoryMock.Object,
                _projetRepositoryMock.Object,
                _pieceRepositoryMock.Object,
                _auditLoggerMock.Object
            );
        }

        [Fact]
        public async Task CreateOrdre_WithValidData_ReturnsOrdre()
        {
            // Arrange
            var request = new CreateOrdreRequest
            {
                ProjetId = 1,
                PieceId = 1,
                Quantite = 10,
                Priorite = OrdrePriorite.Normale
            };

            var projet = new Projet { Id = 1, Nom = "Projet Test" };
            var piece = new Piece { Id = 1, Nom = "Piece Test" };
            var ordre = new OrdreFabrication
            {
                Id = 1,
                Reference = "OF-0001",
                Statut = OrdreStatut.EnAttente,
                Quantite = 10,
                Projet = projet,
                Piece = piece
            };

            _projetRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(projet);
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _ordreRepositoryMock.Setup(x => x.GetNextReferenceNumberAsync()).ReturnsAsync(1);
            _ordreRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<OrdreFabrication>())).ReturnsAsync(ordre);

            // Act
            var result = await _ordreService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EnAttente", result.Statut.ToString());
            Assert.Equal(10, result.Quantite);
            Assert.Equal("OF-0001", result.Reference);
        }

        [Fact]
        public async Task CreateOrdre_WithDevisId_PassesDevisIdThrough()
        {
            // Arrange
            var request = new CreateOrdreRequest
            {
                ProjetId = 1,
                PieceId = 1,
                DevisId = 42,
                Quantite = 3
            };

            var projet = new Projet { Id = 1, Nom = "Projet Test" };
            var piece = new Piece { Id = 1, Nom = "Piece Test" };

            _projetRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(projet);
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _ordreRepositoryMock.Setup(x => x.GetNextReferenceNumberAsync()).ReturnsAsync(1);
            _ordreRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<OrdreFabrication>()))
                .ReturnsAsync((OrdreFabrication o) => o);

            // Act
            var result = await _ordreService.CreateAsync(request);

            // Assert
            Assert.Equal(42, result.DevisId);
            _ordreRepositoryMock.Verify(x => x.CreateAsync(It.Is<OrdreFabrication>(o => o.DevisId == 42)), Times.Once);
        }

        [Fact]
        public async Task ExistsForDevis_DelegatesToRepository()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.ExistsForDevisAsync(42)).ReturnsAsync(true);

            // Act
            var result = await _ordreService.ExistsForDevisAsync(42);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateOrdre_WithInvalidProjet_ThrowsException()
        {
            // Arrange
            var request = new CreateOrdreRequest { ProjetId = 999, PieceId = 1, Quantite = 10 };
            _projetRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Projet?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _ordreService.CreateAsync(request)
            );
        }

        [Fact]
        public async Task CreateOrdre_WithInvalidPiece_ThrowsException()
        {
            // Arrange
            var projet = new Projet { Id = 1, Nom = "Projet Test" };
            var request = new CreateOrdreRequest { ProjetId = 1, PieceId = 999, Quantite = 10 };

            _projetRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(projet);
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Piece?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _ordreService.CreateAsync(request)
            );
        }

        [Fact]
        public async Task GetOrdreById_ExistingOrdre_ReturnsOrdre()
        {
            // Arrange
            var ordre = new OrdreFabrication
            {
                Id = 1,
                Reference = "OF-0001",
                Statut = OrdreStatut.EnAttente
            };
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(ordre);

            // Act
            var result = await _ordreService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("OF-0001", result.Reference);
        }

        [Fact]
        public async Task GetOrdreById_NonExistingOrdre_ReturnsNull()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((OrdreFabrication?)null);

            // Act
            var result = await _ordreService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllOrdres_ReturnsListOfOrdres()
        {
            // Arrange
            var ordres = new List<OrdreFabrication>
            {
                new OrdreFabrication { Id = 1, Reference = "OF-0001" },
                new OrdreFabrication { Id = 2, Reference = "OF-0002" }
            };
            _ordreRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(ordres);

            // Act
            var result = await _ordreService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateOrdre_WithValidData_ReturnsUpdatedOrdre()
        {
            // Arrange
            var existingOrdre = new OrdreFabrication
            {
                Id = 1,
                Reference = "OF-0001",
                Quantite = 5,
                Priorite = OrdrePriorite.Normale
            };
            var updateRequest = new UpdateOrdreRequest
            {
                Quantite = 15,
                Priorite = OrdrePriorite.Haute
            };

            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingOrdre);
            _ordreRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<OrdreFabrication>()))
                .ReturnsAsync((OrdreFabrication o) => { o.Quantite = 15; o.Priorite = OrdrePriorite.Haute; return o; });

            // Act
            var result = await _ordreService.UpdateAsync(1, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.Quantite);
            Assert.Equal("Haute", result.Priorite.ToString());
        }

        [Fact]
        public async Task UpdateOrdre_NonExistingOrdre_ReturnsNull()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((OrdreFabrication?)null);
            var updateRequest = new UpdateOrdreRequest { Quantite = 15, Priorite = OrdrePriorite.Haute };

            // Act
            var result = await _ordreService.UpdateAsync(99, updateRequest);

            // Assert
            Assert.Null(result);
            _ordreRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdreFabrication>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatut_NonExistingOrdre_ReturnsNull()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((OrdreFabrication?)null);

            // Act
            var result = await _ordreService.UpdateStatutAsync(99, OrdreStatut.EnCours);

            // Assert
            Assert.Null(result);
            _ordreRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdreFabrication>()), Times.Never);
        }

        [Fact]
        public async Task StartProduction_NonExistingOrdre_ReturnsNull()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((OrdreFabrication?)null);

            // Act
            var result = await _ordreService.StartProductionAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CompleteProduction_NonExistingOrdre_ReturnsNull()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((OrdreFabrication?)null);

            // Act
            var result = await _ordreService.CompleteProductionAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateStatut_ValidTransition_UpdatesStatus()
        {
            // Arrange
            var ordre = new OrdreFabrication
            {
                Id = 1,
                Reference = "OF-0001",
                Statut = OrdreStatut.EnAttente
            };

            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(ordre);
            _ordreRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<OrdreFabrication>()))
                .ReturnsAsync((OrdreFabrication o) => { o.Statut = OrdreStatut.EnCours; return o; });

            // Act
            var result = await _ordreService.UpdateStatutAsync(1, OrdreStatut.EnCours);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EnCours", result.Statut.ToString());
        }

        [Fact]
        public async Task StartProduction_UpdatesStatusToEnCours()
        {
            // Arrange
            var ordre = new OrdreFabrication
            {
                Id = 1,
                Reference = "OF-0001",
                Statut = OrdreStatut.EnAttente
            };

            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(ordre);
            _ordreRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<OrdreFabrication>()))
                .ReturnsAsync((OrdreFabrication o) => { o.Statut = OrdreStatut.EnCours; return o; });

            // Act
            var result = await _ordreService.StartProductionAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EnCours", result.Statut.ToString());
            Assert.NotNull(result.DateDebut);
        }

        [Fact]
        public async Task CompleteProduction_UpdatesStatusToTermine()
        {
            // Arrange
            var ordre = new OrdreFabrication
            {
                Id = 1,
                Reference = "OF-0001",
                Statut = OrdreStatut.EnCours,
                Quantite = 10,
                QuantiteProduite = 0
            };

            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(ordre);
            _ordreRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<OrdreFabrication>()))
                .ReturnsAsync((OrdreFabrication o) => { o.Statut = OrdreStatut.Termine; o.QuantiteProduite = o.Quantite; return o; });

            // Act
            var result = await _ordreService.CompleteProductionAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Termine", result.Statut.ToString());
            Assert.Equal(10, result.QuantiteProduite);
        }

        [Fact]
        public async Task DeleteOrdre_ExistingOrdre_ReturnsTrue()
        {
            // Arrange
            var ordre = new OrdreFabrication { Id = 1, Reference = "OF-0001" };
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(ordre);
            _ordreRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _ordreService.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteOrdre_NonExistingOrdre_ReturnsFalse()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((OrdreFabrication?)null);

            // Act
            var result = await _ordreService.DeleteAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetStatistics_ReturnsCorrectStats()
        {
            // Arrange
            var ordres = new List<OrdreFabrication>
            {
                new OrdreFabrication { Id = 1, Statut = OrdreStatut.EnAttente, Quantite = 10, QuantiteProduite = 0 },
                new OrdreFabrication { Id = 2, Statut = OrdreStatut.EnCours, Quantite = 5, QuantiteProduite = 2 },
                new OrdreFabrication { Id = 3, Statut = OrdreStatut.Termine, Quantite = 8, QuantiteProduite = 8 }
            };

            _ordreRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(ordres);

            // Act
            var result = await _ordreService.GetStatisticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalOrdres);
            Assert.Equal(1, result.EnAttente);
            Assert.Equal(1, result.EnCours);
            Assert.Equal(1, result.Termines);
            Assert.Equal(23, result.QuantiteTotale); // 10+5+8
            Assert.Equal(10, result.QuantiteProduite); // 0+2+8
        }

        [Fact]
        public async Task GetStatistics_WithNoOrdres_ReturnsZeroedStatsWithoutDivisionByZero()
        {
            // Arrange
            _ordreRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<OrdreFabrication>());

            // Act
            var result = await _ordreService.GetStatisticsAsync();

            // Assert
            Assert.Equal(0, result.TotalOrdres);
            Assert.Equal(0, result.TauxAvancement);
        }
    }
}
