using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace TestProject
{
    public class ClientPortalAuthServiceTests
    {
        private readonly IClientPortalAuthService _service;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IClientMagicLinkRepository> _magicLinkRepositoryMock;
        private readonly Mock<IClientPortalMailSender> _mailSenderMock;
        private readonly Mock<IConfiguration> _configMock;

        public ClientPortalAuthServiceTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _magicLinkRepositoryMock = new Mock<IClientMagicLinkRepository>();
            _mailSenderMock = new Mock<IClientPortalMailSender>();
            _configMock = new Mock<IConfiguration>();

            _configMock.Setup(c => c["Jwt:Key"]).Returns("c6f0b8d2e5a47f39d1e8c2a9f7b4d6e8");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("PrintFlow3D");
            _configMock.Setup(c => c["ClientPortal:JwtAudience"]).Returns("PrintFlow3DClientPortal");
            _configMock.Setup(c => c["ClientPortal:FrontendBaseUrl"]).Returns("http://localhost:4200");
            _configMock.Setup(c => c["ClientPortal:LinkExpiryMinutes"]).Returns("30");
            _configMock.Setup(c => c["ClientPortal:SessionExpiryHours"]).Returns("2");

            _service = new ClientPortalAuthService(
                _clientRepositoryMock.Object,
                _magicLinkRepositoryMock.Object,
                _mailSenderMock.Object,
                _configMock.Object
            );
        }

        [Fact]
        public async Task RequestAccess_WithExistingClient_CreatesLinkAndSendsMail()
        {
            var client = new Client { Id = 7, Nom = "Test SARL", Email = "contact@testsarl.com" };
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync("contact@testsarl.com")).ReturnsAsync(client);

            await _service.RequestAccessAsync("contact@testsarl.com");

            _magicLinkRepositoryMock.Verify(x => x.CreateAsync(It.Is<ClientMagicLink>(l =>
                l.ClientId == 7 && !string.IsNullOrEmpty(l.TokenHash) && l.ExpiresAt > DateTime.UtcNow)), Times.Once);
            _mailSenderMock.Verify(x => x.SendMagicLinkAsync("contact@testsarl.com", "Test SARL", It.Is<string>(url => url.Contains("token="))), Times.Once);
        }

        [Fact]
        public async Task RequestAccess_WithNonExistingClient_DoesNothingSilently()
        {
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync("nobody@nowhere.test")).ReturnsAsync((Client?)null);

            await _service.RequestAccessAsync("nobody@nowhere.test");

            _magicLinkRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ClientMagicLink>()), Times.Never);
            _mailSenderMock.Verify(x => x.SendMagicLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RequestAccess_WithEmptyEmail_DoesNothingSilently()
        {
            await _service.RequestAccessAsync("");

            _clientRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            _magicLinkRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ClientMagicLink>()), Times.Never);
        }

        [Fact]
        public async Task Consume_WithValidToken_ReturnsAuthResponseAndMarksConsumed()
        {
            var client = new Client { Id = 7, Nom = "Test SARL", Email = "contact@testsarl.com" };
            var link = new ClientMagicLink
            {
                Id = 1,
                ClientId = 7,
                TokenHash = "irrelevant-in-this-test",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                ConsumedAt = null,
                Client = client
            };
            _magicLinkRepositoryMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(link);

            var result = await _service.ConsumeAsync("some-raw-token");

            Assert.NotNull(result);
            Assert.Equal(7, result.ClientId);
            Assert.Equal("Test SARL", result.ClientNom);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.NotNull(link.ConsumedAt);
            _magicLinkRepositoryMock.Verify(x => x.UpdateAsync(It.Is<ClientMagicLink>(l => l.ConsumedAt.HasValue)), Times.Once);
        }

        [Fact]
        public async Task Consume_WithUnknownToken_ReturnsNull()
        {
            _magicLinkRepositoryMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync((ClientMagicLink?)null);

            var result = await _service.ConsumeAsync("unknown-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task Consume_WithAlreadyConsumedToken_ReturnsNull()
        {
            var link = new ClientMagicLink
            {
                Id = 1,
                ClientId = 7,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                ConsumedAt = DateTime.UtcNow.AddMinutes(-5),
                Client = new Client { Id = 7, Nom = "Test SARL", Email = "contact@testsarl.com" }
            };
            _magicLinkRepositoryMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(link);

            var result = await _service.ConsumeAsync("already-used-token");

            Assert.Null(result);
            _magicLinkRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ClientMagicLink>()), Times.Never);
        }

        [Fact]
        public async Task Consume_WithExpiredToken_ReturnsNull()
        {
            var link = new ClientMagicLink
            {
                Id = 1,
                ClientId = 7,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                ConsumedAt = null,
                Client = new Client { Id = 7, Nom = "Test SARL", Email = "contact@testsarl.com" }
            };
            _magicLinkRepositoryMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(link);

            var result = await _service.ConsumeAsync("expired-token");

            Assert.Null(result);
            _magicLinkRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ClientMagicLink>()), Times.Never);
        }

        [Fact]
        public async Task Consume_WithEmptyToken_ReturnsNull()
        {
            var result = await _service.ConsumeAsync("");

            Assert.Null(result);
            _magicLinkRepositoryMock.Verify(x => x.GetByTokenHashAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
