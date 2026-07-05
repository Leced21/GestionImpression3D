using Backend.Controllers;
using Backend.Interface;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace TestProject
{
    // Le point le plus critique du portail client : un client authentifié ne doit jamais
    // pouvoir lire un devis/facture/commande appartenant à un AUTRE client, même en devinant
    // un id. Le contrôleur doit toujours vérifier l'appartenance via le ClientId du token,
    // jamais faire confiance à un paramètre fourni par l'appelant.
    public class ClientPortalControllerTests
    {
        private readonly Mock<IDevisService> _devisServiceMock;
        private readonly Mock<IFactureService> _factureServiceMock;
        private readonly Mock<ICommercialService> _commercialServiceMock;
        private readonly ClientPortalController _controller;

        public ClientPortalControllerTests()
        {
            _devisServiceMock = new Mock<IDevisService>();
            _factureServiceMock = new Mock<IFactureService>();
            _commercialServiceMock = new Mock<ICommercialService>();

            _controller = new ClientPortalController(
                _devisServiceMock.Object,
                _factureServiceMock.Object,
                _commercialServiceMock.Object
            );

            SetCurrentClient(clientId: 7);
        }

        private void SetCurrentClient(int clientId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, clientId.ToString()) };
            var identity = new ClaimsIdentity(claims, "ClientPortal");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetDevisById_BelongingToAnotherClient_ReturnsNotFound()
        {
            var devis = new Devis { Id = 1, ClientId = 99 };
            _devisServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);

            var result = await _controller.GetDevisById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDevisById_BelongingToCurrentClient_ReturnsOk()
        {
            var devis = new Devis { Id = 1, ClientId = 7 };
            _devisServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(devis);

            var result = await _controller.GetDevisById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(devis, okResult.Value);
        }

        [Fact]
        public async Task GetDevisById_NonExisting_ReturnsNotFound()
        {
            _devisServiceMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Devis?)null);

            var result = await _controller.GetDevisById(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFactureById_BelongingToAnotherClient_ReturnsNotFound()
        {
            var facture = new Facture { Id = 1, ClientId = 99 };
            _factureServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(facture);

            var result = await _controller.GetFactureById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFactureById_BelongingToCurrentClient_ReturnsOk()
        {
            var facture = new Facture { Id = 1, ClientId = 7 };
            _factureServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(facture);

            var result = await _controller.GetFactureById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(facture, okResult.Value);
        }

        [Fact]
        public async Task GetFacturePdf_BelongingToAnotherClient_ReturnsNotFound()
        {
            var facture = new Facture { Id = 1, ClientId = 99 };
            _factureServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(facture);

            var result = await _controller.GetFacturePdf(1);

            Assert.IsType<NotFoundResult>(result);
            _factureServiceMock.Verify(x => x.GeneratePdfAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCommandeById_BelongingToAnotherClient_ReturnsNotFound()
        {
            var commande = new Commande { Id = 1, ClientId = 99 };
            _commercialServiceMock.Setup(x => x.GetCommandeAsync(1)).ReturnsAsync(commande);

            var result = await _controller.GetCommandeById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCommandeById_BelongingToCurrentClient_ReturnsOk()
        {
            var commande = new Commande { Id = 1, ClientId = 7 };
            _commercialServiceMock.Setup(x => x.GetCommandeAsync(1)).ReturnsAsync(commande);

            var result = await _controller.GetCommandeById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(commande, okResult.Value);
        }

        [Fact]
        public async Task GetDevis_ReturnsOnlyCurrentClientDevisFromService()
        {
            var devis = new List<Devis> { new Devis { Id = 1, ClientId = 7 } };
            _devisServiceMock.Setup(x => x.GetByClientAsync(7)).ReturnsAsync(devis);

            var result = await _controller.GetDevis();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(devis, okResult.Value);
            _devisServiceMock.Verify(x => x.GetByClientAsync(7), Times.Once);
        }
    }
}
