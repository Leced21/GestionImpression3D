using Backend.DTOs;
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

        public CommercialServiceTests()
        {
            _pieceRepositoryMock = new Mock<IPieceRepository>();
            _commercialRepositoryMock = new Mock<ICommercialRepository>();
            _clientServiceMock = new Mock<IClientService>();

            _commercialService = new CommercialService(
                _pieceRepositoryMock.Object,
                _commercialRepositoryMock.Object,
                _clientServiceMock.Object
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
            _commercialRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Commande>())).ReturnsAsync((Commande c) => c);

            // Act
            var result = await _commercialService.CreerCommandeAsync(request);

            // Assert
            Assert.Equal(5, result.ClientId);
            _commercialRepositoryMock.Verify(x => x.CreateAsync(It.Is<Commande>(c => c.ClientId == 5)), Times.Once);
        }

        [Fact]
        public async Task CreerCommande_WhenClientResolutionReturnsNull_FallsBackToRequestFieldsWithoutClientId()
        {
            // Arrange : EnsureClientAsync peut renvoyer null (ex: nom et email vides)
            var request = CreateRequest();
            var piece = new Piece { Id = 1, Nom = "Piece Test", Reference = "REF-1", PrixVente = 15m, Stock = 10 };

            _pieceRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(piece);
            _clientServiceMock.Setup(x => x.EnsureClientAsync(It.IsAny<CreateClientRequest>())).ReturnsAsync((Client?)null);
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
    }
}
