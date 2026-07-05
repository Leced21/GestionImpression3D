using Backend.Data;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TestProject
{
    public class ClientRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ClientRepository _repository;

        public ClientRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"GestionImpression3D_ClientRepositoryTests_{Guid.NewGuid():N}")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
            _repository = new ClientRepository(_context);
        }

        [Fact]
        public async Task SearchAsync_WithNullSiret_DoesNotThrowAndReturnsMatchingClient()
        {
            _context.Clients.Add(new Client
            {
                Nom = "Atelier Test",
                Email = "atelier@test.local",
                Siret = null,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var results = await _repository.SearchAsync("Atelier");

            var client = Assert.Single(results);
            Assert.Equal("Atelier Test", client.Nom);
        }

        [Fact]
        public async Task SearchAsync_WithSiretTerm_ReturnsMatchingClient()
        {
            _context.Clients.AddRange(
                new Client
                {
                    Nom = "Siret Match",
                    Email = "match@test.local",
                    Siret = "12345678900011",
                    CreatedAt = DateTime.UtcNow
                },
                new Client
                {
                    Nom = "Other Client",
                    Email = "other@test.local",
                    Siret = "99999999900099",
                    CreatedAt = DateTime.UtcNow
                }
            );
            await _context.SaveChangesAsync();

            var results = await _repository.SearchAsync("123456789");

            var client = Assert.Single(results);
            Assert.Equal("Siret Match", client.Nom);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingClient_ReturnsClient()
        {
            var client = new Client { Nom = "Test Client", Email = "test@test.local", CreatedAt = DateTime.UtcNow };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(client.Id);

            Assert.NotNull(result);
            Assert.Equal("Test Client", result.Nom);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingClient_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_ExistingClient_ReturnsClient()
        {
            _context.Clients.Add(new Client { Nom = "Test Client", Email = "test@test.local", CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var result = await _repository.GetByEmailAsync("test@test.local");

            Assert.NotNull(result);
            Assert.Equal("test@test.local", result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_NonExistingClient_ReturnsNull()
        {
            var result = await _repository.GetByEmailAsync("nobody@test.local");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsClientsOrderedByNom()
        {
            _context.Clients.AddRange(
                new Client { Nom = "Zebra SARL", Email = "zebra@test.local", CreatedAt = DateTime.UtcNow },
                new Client { Nom = "Alpha SARL", Email = "alpha@test.local", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var results = (await _repository.GetAllAsync()).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal("Alpha SARL", results[0].Nom);
            Assert.Equal("Zebra SARL", results[1].Nom);
        }

        [Fact]
        public async Task CreateAsync_AddsClientAndSetsCreatedAt()
        {
            var client = new Client { Nom = "New Client", Email = "new@test.local" };

            var result = await _repository.CreateAsync(client);

            Assert.True(result.Id > 0);
            Assert.NotEqual(default, result.CreatedAt);
            Assert.Equal(1, await _context.Clients.CountAsync());
        }

        [Fact]
        public async Task UpdateAsync_ModifiesExistingClientAndSetsUpdatedAt()
        {
            var client = new Client { Nom = "Old Name", Email = "test@test.local", CreatedAt = DateTime.UtcNow };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _context.Entry(client).State = EntityState.Detached;

            client.Nom = "New Name";
            var result = await _repository.UpdateAsync(client);

            Assert.Equal("New Name", result.Nom);
            Assert.NotNull(result.UpdatedAt);

            var reloaded = await _context.Clients.FindAsync(client.Id);
            Assert.Equal("New Name", reloaded!.Nom);
        }

        [Fact]
        public async Task DeleteAsync_ExistingClient_RemovesClientAndReturnsTrue()
        {
            var client = new Client { Nom = "To Delete", Email = "delete@test.local", CreatedAt = DateTime.UtcNow };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteAsync(client.Id);

            Assert.True(result);
            Assert.Equal(0, await _context.Clients.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_NonExistingClient_ReturnsFalse()
        {
            var result = await _repository.DeleteAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task GetCountAsync_ReturnsCorrectCount()
        {
            _context.Clients.AddRange(
                new Client { Nom = "Client 1", Email = "c1@test.local", CreatedAt = DateTime.UtcNow },
                new Client { Nom = "Client 2", Email = "c2@test.local", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetCountAsync();

            Assert.Equal(2, result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
