using Backend.Data;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TestProject
{
    public class CommercialRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly CommercialRepository _repository;

        public CommercialRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"GestionImpression3D_CommercialRepositoryTests_{Guid.NewGuid():N}")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
            _repository = new CommercialRepository(_context);
        }

        [Fact]
        public async Task GenerateNumeroCommandeAsync_WithNoExistingCommandeThisYear_ReturnsFirstNumber()
        {
            var numero = await _repository.GenerateNumeroCommandeAsync();

            var year = DateTime.Now.Year;
            Assert.Equal($"CMD-{year}-0001", numero);
            Assert.True(numero.Length <= 20, "NumeroCommande doit tenir dans la colonne nvarchar(20)");
        }

        [Fact]
        public async Task GenerateNumeroCommandeAsync_WithExistingCommandeThisYear_IncrementsSequence()
        {
            var year = DateTime.Now.Year;
            _context.Commandes.Add(new Commande
            {
                NumeroCommande = $"CMD-{year}-0007",
                DateCommande = DateTime.Now
            });
            await _context.SaveChangesAsync();

            var numero = await _repository.GenerateNumeroCommandeAsync();

            Assert.Equal($"CMD-{year}-0008", numero);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
