using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;


namespace TestProject
{
    public class PieceServiceTests
    {
        private readonly IPieceService _pieceService;
        private readonly Mock<IPieceRepository> _pieceRepositoryMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;
        private readonly Mock<IPieceVersionRepository> _pieceVersionRepositoryMock;
        private readonly Mock<ISTLAnalyzerService> _stlAnalyzerServiceMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        public PieceServiceTests()
        {
            _pieceRepositoryMock = new Mock<IPieceRepository>();
            _auditLoggerMock = new Mock<IAuditLogger>();
            _pieceVersionRepositoryMock = new Mock<IPieceVersionRepository>();
            _stlAnalyzerServiceMock = new Mock<ISTLAnalyzerService>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _pieceService = new PieceService(
                _pieceRepositoryMock.Object,
                _auditLoggerMock.Object,
                _pieceVersionRepositoryMock.Object,
                _stlAnalyzerServiceMock.Object,
                _serviceProviderMock.Object
            );
        }

        [Fact]
        public async Task CreatePiece_WithValidData_ReturnsPiece()
        {
            // Arrange
            var piece = new Piece { Nom = "Test Piece", Reference = "TEST-001" };
            var createdPiece = new Piece
            {
                Id = 1,
                Nom = "Test Piece",
                Reference = "TEST-001",
                Statut = PieceStatus.Brouillon,
                DateCreation = DateTime.Now
            };

            _pieceRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Piece>())).ReturnsAsync(createdPiece);
            _pieceVersionRepositoryMock.Setup(x => x.GetNextVersionNumberAsync(It.IsAny<int>())).ReturnsAsync(1);
            _pieceVersionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<PieceVersion>())).ReturnsAsync(new PieceVersion());

            // Act
            var result = await _pieceService.CreateAsync(piece);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Piece", result.Nom);
            Assert.Equal("Brouillon", result.Statut.ToString());
        }

        [Fact]
        public async Task UpdateStatut_ValidTransition_UpdatesStatus()
        {
            // Arrange
            var piece = new Piece
            {
                Id = 1,
                Nom = "Test",
                Statut = PieceStatus.Brouillon
            };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _pieceRepositoryMock.Setup(x => x.UpdateStatutAsync(It.IsAny<int>(), It.IsAny<PieceStatus>()))
                .ReturnsAsync((int id, PieceStatus p) =>
            {
                piece.Statut = p;
                return piece;
            });

            // Act
            var result = await _pieceService.UpdateStatutAsync(1, PieceStatus.Conception);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Conception", result.Statut.ToString());
            _pieceRepositoryMock.Verify(x => x.UpdateStatutAsync(1, PieceStatus.Conception), Times.Once);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(EntityType.Piece, 1, "Brouillon", "Conception"), Times.Once);
        }

        [Fact]
        public async Task UpdateStatut_InvalidTransition_ThrowsException()
        {
            // Arrange
            var piece = new Piece
            {
                Id = 1,
                Nom = "Test",
                Statut = PieceStatus.Conception
            };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _pieceService.UpdateStatutAsync(1, PieceStatus.Production)
            );
            _pieceRepositoryMock.Verify(x => x.UpdateStatutAsync(It.IsAny<int>(), It.IsAny<PieceStatus>()), Times.Never);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatut_NonExistingPiece_ReturnsNull()
        {
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Piece?)null);

            var result = await _pieceService.UpdateStatutAsync(99, PieceStatus.Conception);

            Assert.Null(result);
            _pieceRepositoryMock.Verify(x => x.UpdateStatutAsync(It.IsAny<int>(), It.IsAny<PieceStatus>()), Times.Never);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePiece_NonExistingPiece_ReturnsNull()
        {
            // Arrange
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Piece?)null);
            var updatedPiece = new Piece { Nom = "Updated" };

            // Act
            var result = await _pieceService.UpdateAsync(99, updatedPiece);

            // Assert
            Assert.Null(result);
            _pieceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Piece>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePiece_WithChangedFields_LogsEachChangeAndUpdates()
        {
            // Arrange
            var existingPiece = new Piece
            {
                Id = 1,
                Nom = "Old Name",
                PrixVente = 10m,
                Description = "Old Description",
                DateCreation = new DateTime(2026, 1, 1)
            };
            var updatedPiece = new Piece
            {
                Nom = "New Name",
                PrixVente = 20m,
                Description = "New Description"
            };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingPiece);
            _pieceRepositoryMock.Setup(x => x.UpdateAsync(1, It.IsAny<Piece>())).ReturnsAsync((int id, Piece p) => p);

            // Act
            var result = await _pieceService.UpdateAsync(1, updatedPiece);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Nom);
            Assert.Equal(existingPiece.DateCreation, result.DateCreation);
            _auditLoggerMock.Verify(x => x.LogUpdateAsync(EntityType.Piece, 1, "Nom", "Old Name", "New Name"), Times.Once);
            _auditLoggerMock.Verify(x => x.LogUpdateAsync(EntityType.Piece, 1, "PrixVente", "10.00", "20.00"), Times.Once);
            _auditLoggerMock.Verify(x => x.LogUpdateAsync(EntityType.Piece, 1, "Description", "Old Description", "New Description"), Times.Once);
        }

        [Fact]
        public async Task UpdatePiece_WithNoChangedFields_DoesNotLogAnything()
        {
            // Arrange
            var existingPiece = new Piece
            {
                Id = 1,
                Nom = "Same Name",
                PrixVente = 10m,
                Description = "Same Description"
            };
            var updatedPiece = new Piece
            {
                Nom = "Same Name",
                PrixVente = 10m,
                Description = "Same Description"
            };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingPiece);
            _pieceRepositoryMock.Setup(x => x.UpdateAsync(1, It.IsAny<Piece>())).ReturnsAsync((int id, Piece p) => p);

            // Act
            await _pieceService.UpdateAsync(1, updatedPiece);

            // Assert
            _auditLoggerMock.Verify(x => x.LogUpdateAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CalculerPrixRecommande_DelegatesToRepository()
        {
            // Arrange
            _pieceRepositoryMock.Setup(x => x.CalculerPrixRecommandéAsync(1)).ReturnsAsync(42.5m);

            // Act
            var result = await _pieceService.CalculerPrixRecommandéAsync(1);

            // Assert
            Assert.Equal(42.5m, result);
        }

        [Fact]
        public async Task GetById_ExistingPiece_ReturnsPiece()
        {
            // Arrange
            var piece = new Piece { Id = 1, Nom = "Test Piece", Statut = PieceStatus.Production };
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);

            // Act
            var result = await _pieceService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Piece", result.Nom);
        }

        [Fact]
        public async Task GetById_NonExistingPiece_ReturnsNull()
        {
            // Arrange
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Piece?)null);

            // Act
            var result = await _pieceService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeletePiece_ExistingPiece_ReturnsTrue()
        {
            // Arrange
            var piece = new Piece { Id = 1, Nom = "Test Piece" };
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _pieceRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _pieceService.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeletePiece_NonExistingPiece_ReturnsFalse()
        {
            // Arrange
            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Piece?)null);

            // Act
            var result = await _pieceService.DeleteAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllPieces_ReturnsListOfPieces()
        {
            // Arrange
            var pieces = new List<Piece>
            {
                new Piece { Id = 1, Nom = "Piece 1" },
                new Piece { Id = 2, Nom = "Piece 2" }
            };
            _pieceRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(pieces);

            // Act
            var result = await _pieceService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }
}
