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
            var databaseName = $"GestionImpression3D_ClientRepositoryTests_{Guid.NewGuid():N}";
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer($@"Server=(localdb)\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true")
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

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
