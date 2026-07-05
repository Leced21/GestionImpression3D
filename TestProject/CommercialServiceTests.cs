using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;

namespace TestProject
{
    public class CommercialServiceTests
    {
        private readonly ICommercialService _commercialService;
        private readonly Mock<IPieceRepository> _pieceRepositoryMock;
        private readonly Mock<ICommercialRepository> _commercialRepositoryMock;
        private readonly Mock<IClientService> _clientServiceMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;

        public CommercialServiceTests()
        {
            _pieceRepositoryMock = new Mock<IPieceRepository>();
            _commercialRepositoryMock = new Mock<ICommercialRepository>();
            _clientServiceMock = new Mock<IClientService>();
            _auditLoggerMock = new Mock<IAuditLogger>();

            _commercialService = new CommercialService(
                _pieceRepositoryMock.Object,
                _commercialRepositoryMock.Object,
                _clientServiceMock.Object,
                _auditLoggerMock.Object
            );
        }

        private static CommandeRequest CreateRequest()
        {
            return new CommandeRequest
            {
                ClientNom = "Test SARL",
                ClientEmail = "contact@testsarl.com",
                ClientTelephone = "0123456789",
                AdresseLivraison = "1 rue de la Paix",
                Items = new List<CommandeItem>
                {
                    new CommandeItem { PieceId = 1, Quantite = 2 }
                }
            };
        }

        [Fact]
        public async Task CreerCommande_WithResolvedClient_SetsClientIdOnCommande()
        {
            // Arrange
            var request = CreateRequest();
            var piece = new Piece { Id = 1, Nom = "Piece Test", Reference = "REF-1", PrixVente = 15m, Stock = 10 };
            var client = new Client { Id = 5, Nom = "Test SARL", Email = "contact@testsarl.com" };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _clientServiceMock.Setup(x => x.EnsureClientAsync(It.IsAny<CreateClientRequest>())).ReturnsAsync(client);
            _commercialRepositoryMock.Setup(x => x.GenerateNumeroCommandeAsync()).ReturnsAsync("CMD-2026-0001");
            _commercialRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Commande>())).ReturnsAsync((Commande c) => c);

            // Act
            var result = await _commercialService.CreerCommandeAsync(request);

            // Assert
            Assert.Equal(5, result.ClientId);
            Assert.Equal("CMD-2026-0001", result.NumeroCommande);
            _commercialRepositoryMock.Verify(x => x.CreateAsync(It.Is<Commande>(c => c.ClientId == 5)), Times.Once);
            _auditLoggerMock.Verify(x => x.LogCreationAsync(EntityType.Commande, It.IsAny<int>(), "CMD-2026-0001"), Times.Once);
        }

        [Fact]
        public async Task CreerCommande_WhenClientResolutionReturnsNull_FallsBackToRequestFieldsWithoutClientId()
        {
            // Arrange : EnsureClientAsync peut renvoyer null (ex: nom et email vides)
            var request = CreateRequest();
            var piece = new Piece { Id = 1, Nom = "Piece Test", Reference = "REF-1", PrixVente = 15m, Stock = 10 };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _clientServiceMock.Setup(x => x.EnsureClientAsync(It.IsAny<CreateClientRequest>())).ReturnsAsync((Client?)null);
            _commercialRepositoryMock.Setup(x => x.GenerateNumeroCommandeAsync()).ReturnsAsync("CMD-2026-0002");
            _commercialRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Commande>())).ReturnsAsync((Commande c) => c);

            // Act
            var result = await _commercialService.CreerCommandeAsync(request);

            // Assert
            Assert.Null(result.ClientId);
            Assert.Equal(request.ClientNom, result.ClientNom);
            Assert.Equal(request.ClientEmail, result.ClientEmail);
        }

        [Fact]
        public async Task CreerCommande_WithInsufficientStock_ThrowsException()
        {
            // Arrange
            var request = CreateRequest();
            var piece = new Piece { Id = 1, Nom = "Piece Test", Stock = 1 };
            var client = new Client { Id = 5, Nom = "Test SARL", Email = "contact@testsarl.com" };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _clientServiceMock.Setup(x => x.EnsureClientAsync(It.IsAny<CreateClientRequest>())).ReturnsAsync(client);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _commercialService.CreerCommandeAsync(request)
            );
            _commercialRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Commande>()), Times.Never);
        }

        [Fact]
        public async Task CreerCommande_WithoutItems_ThrowsException()
        {
            // Arrange
            var request = CreateRequest();
            request.Items = new List<CommandeItem>();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _commercialService.CreerCommandeAsync(request)
            );
            _clientServiceMock.Verify(x => x.EnsureClientAsync(It.IsAny<CreateClientRequest>()), Times.Never);
        }

        [Fact]
        public async Task GetByClient_DelegatesToRepository()
        {
            // Arrange
            var commandes = new List<Commande>
            {
                new Commande { Id = 1, ClientId = 5, NumeroCommande = "CMD-1" }
            };
            _commercialRepositoryMock.Setup(x => x.GetByClientAsync(5)).ReturnsAsync(commandes);

            // Act
            var result = await _commercialService.GetByClientAsync(5);

            // Assert
            Assert.Single(result);
        }

        private static Commande CreateCommandeEnAttente()
        {
            return new Commande
            {
                Id = 1,
                NumeroCommande = "CMD-2026-0001",
                Statut = CommandeStatus.EnAttente,
                Lignes = new List<CommandeLigne>
                {
                    new CommandeLigne { PieceId = 10, Quantite = 3 },
                    new CommandeLigne { PieceId = 11, Quantite = 1 }
                }
            };
        }

        [Fact]
        public async Task AnnulerCommande_WithPendingCommande_RestoresStockAndSetsStatutAnnulee()
        {
            var commande = CreateCommandeEnAttente();
            _commercialRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(commande);
            _commercialRepositoryMock.Setup(x => x.UpdateStatutAsync(1, CommandeStatus.Annulée))
                .ReturnsAsync(new Commande { Id = 1, Statut = CommandeStatus.Annulée });

            var result = await _commercialService.AnnulerCommandeAsync(1);

            Assert.True(result);
            _commercialRepositoryMock.Verify(x => x.RestoreStockAsync(10, 3), Times.Once);
            _commercialRepositoryMock.Verify(x => x.RestoreStockAsync(11, 1), Times.Once);
            _commercialRepositoryMock.Verify(x => x.UpdateStatutAsync(1, CommandeStatus.Annulée), Times.Once);
            _commercialRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(EntityType.Commande, 1, "EnAttente", "Annulée"), Times.Once);
        }

        [Fact]
        public async Task AnnulerCommande_WithNonCancellableStatus_ThrowsAndDoesNotRestoreStock()
        {
            var commande = CreateCommandeEnAttente();
            commande.Statut = CommandeStatus.Livrée;
            _commercialRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(commande);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _commercialService.AnnulerCommandeAsync(1));

            _commercialRepositoryMock.Verify(x => x.RestoreStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AnnulerCommande_WithNonExistingCommande_ReturnsFalse()
        {
            _commercialRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Commande?)null);

            var result = await _commercialService.AnnulerCommandeAsync(99);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateStatutCommande_WithInvalidStatut_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _commercialService.UpdateStatutCommandeAsync(1, (CommandeStatus)999));
        }

        [Fact]
        public async Task UpdateStatutCommande_ToSameStatut_IsNoOpAndDoesNotRestoreStock()
        {
            var commande = CreateCommandeEnAttente();
            _commercialRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(commande);

            var result = await _commercialService.UpdateStatutCommandeAsync(1, CommandeStatus.EnAttente);

            Assert.Equal(commande, result);
            _commercialRepositoryMock.Verify(x => x.RestoreStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _commercialRepositoryMock.Verify(x => x.UpdateStatutAsync(It.IsAny<int>(), It.IsAny<CommandeStatus>()), Times.Never);
            _auditLoggerMock.Verify(x => x.LogStatusChangeAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatutCommande_ToProduction_DoesNotRestoreStock()
        {
            var commande = CreateCommandeEnAttente();
            _commercialRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(commande);
            _commercialRepositoryMock.Setup(x => x.UpdateStatutAsync(1, CommandeStatus.EnProduction))
                .ReturnsAsync(new Commande { Id = 1, Statut = CommandeStatus.EnProduction });

            await _commercialService.UpdateStatutCommandeAsync(1, CommandeStatus.EnProduction);

            _commercialRepositoryMock.Verify(x => x.RestoreStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
