using Backend.DTOs;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Moq;

namespace TestProject
{
    public class ClientServiceTests
    {
        private readonly IClientService _clientService;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;

        public ClientServiceTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _auditLoggerMock = new Mock<IAuditLogger>();

            _clientService = new ClientService(
                _clientRepositoryMock.Object,
                _auditLoggerMock.Object
            );
        }
        [Fact]
        public async Task CreateClient_WithValidData_ReturnsClient()
        {
            // Arrange
            var request = new CreateClientRequest
            {
                Nom = "Test SARL",
                Email = "contact@testsarl.com",
                Telephone = "0123456789",
                Adresse = "1 rue de la Paix",
                Ville = "Paris",
                CodePostal = "75001"
            };

            var client = new Client
            {
                Id = 1,
                Nom = "Test SARL",
                Email = "contact@testsarl.com",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Client>())).ReturnsAsync(client);

            // Act
            var result = await _clientService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test SARL", result.Nom);
            Assert.Equal("contact@testsarl.com", result.Email);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task CreateClient_WithExistingEmail_ThrowsException()
        {
            // Arrange
            var existingClient = new Client { Id = 1, Email = "existing@test.com" };
            var request = new CreateClientRequest { Nom = "Test", Email = "existing@test.com" };

            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(existingClient);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _clientService.CreateAsync(request)
            );
        }

        [Fact]
        public async Task GetClientById_ExistingClient_ReturnsClient()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Nom = "Test Client",
                Email = "test@test.com"
            };
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(client);

            // Act
            var result = await _clientService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Client", result.Nom);
        }

        [Fact]
        public async Task GetClientById_NonExistingClient_ReturnsNull()
        {
            // Arrange
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Client?)null);

            // Act
            var result = await _clientService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetClientByEmail_ExistingClient_ReturnsClient()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Nom = "Test Client",
                Email = "test@test.com"
            };
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync("test@test.com")).ReturnsAsync(client);

            // Act
            var result = await _clientService.GetByEmailAsync("test@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
        }

        [Fact]
        public async Task UpdateClient_ExistingClient_ReturnsUpdatedClient()
        {
            // Arrange
            var existingClient = new Client
            {
                Id = 1,
                Nom = "Old Name",
                Email = "old@test.com"
            };
            var updateRequest = new UpdateClientRequest
            {
                Nom = "New Name",
                Email = "new@test.com",
                IsActive = true
            };

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingClient);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>()))
                .ReturnsAsync((Client c) => { c.Nom = "New Name"; c.Email = "new@test.com"; return c; });

            // Act
            var result = await _clientService.UpdateAsync(1, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Nom);
            Assert.Equal("new@test.com", result.Email);
        }

        [Fact]
        public async Task DeleteClient_ExistingClient_ReturnsTrue()
        {
            // Arrange
            var client = new Client { Id = 1, Nom = "Test Client" };
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _clientService.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteClient_NonExistingClient_ReturnsFalse()
        {
            // Arrange
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Client?)null);

            // Act
            var result = await _clientService.DeleteAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllClients_ReturnsListOfClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                new Client { Id = 1, Nom = "Client 1" },
                new Client { Id = 2, Nom = "Client 2" }
            };
            _clientRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(clients);

            // Act
            var result = await _clientService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchClients_WithSearchTerm_ReturnsMatchingClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                new Client { Id = 1, Nom = "Test SARL" },
                new Client { Id = 2, Nom = "Another Company" }
            };
            _clientRepositoryMock.Setup(x => x.SearchAsync("Test")).ReturnsAsync(clients);

            // Act
            var result = await _clientService.SearchAsync("Test");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetClientCount_ReturnsCorrectCount()
        {
            // Arrange
            _clientRepositoryMock.Setup(x => x.GetCountAsync()).ReturnsAsync(5);

            // Act
            var result = await _clientService.GetCountAsync();

            // Assert
            Assert.Equal(5, result);
        }
    }
}
