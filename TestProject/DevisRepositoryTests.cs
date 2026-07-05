using Backend.Data;
using Backend.Enums;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TestProject
{
    public class DevisRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DevisRepository _repository;

        public DevisRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"GestionImpression3D_DevisRepositoryTests_{Guid.NewGuid():N}")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
            _repository = new DevisRepository(_context);
        }

        [Fact]
        public async Task UpdateAsync_ReplacesOldLignesWithNewOnes()
        {
            // Arrange : un devis avec une ligne existante en base
            var client = new Client { Nom = "Test SARL", Email = "contact@testsarl.com", CreatedAt = DateTime.UtcNow };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var devis = new Devis
            {
                NumeroDevis = "DEV-2026-0001",
                ClientId = client.Id,
                DateEmission = DateTime.UtcNow,
                DateValidite = DateTime.UtcNow.AddDays(30),
                Statut = DevisStatus.Brouillon,
                CreatedAt = DateTime.UtcNow,
                Lignes = new List<DevisLigne>
                {
                    new DevisLigne { Description = "Ancienne ligne", Quantite = 1, PrixUnitaire = 10m }
                }
            };
            _context.Devis.Add(devis);
            await _context.SaveChangesAsync();

            var ancienneLigneId = devis.Lignes[0].Id;

            // Act : on récupère le devis tracké (comme le ferait le service), puis on remplace ses lignes
            var tracked = await _repository.GetByIdAsync(devis.Id);
            tracked!.Lignes = new List<DevisLigne>
            {
                new DevisLigne { Description = "Nouvelle ligne A", Quantite = 2, PrixUnitaire = 20m },
                new DevisLigne { Description = "Nouvelle ligne B", Quantite = 1, PrixUnitaire = 5m }
            };
            tracked.TotalHT = 45m;
            tracked.TotalTTC = 54m;

            await _repository.UpdateAsync(tracked);

            // Assert : l'ancienne ligne a bien disparu, seules les deux nouvelles existent en base
            var lignesEnBase = await _context.DevisLignes.Where(l => l.DevisId == devis.Id).ToListAsync();

            Assert.Equal(2, lignesEnBase.Count);
            Assert.DoesNotContain(lignesEnBase, l => l.Id == ancienneLigneId);
            Assert.Contains(lignesEnBase, l => l.Description == "Nouvelle ligne A");
            Assert.Contains(lignesEnBase, l => l.Description == "Nouvelle ligne B");
        }

        [Fact]
        public async Task UpdateAsync_PersistsScalarFieldChanges()
        {
            var client = new Client { Nom = "Test SARL", Email = "contact@testsarl.com", CreatedAt = DateTime.UtcNow };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var devis = new Devis
            {
                NumeroDevis = "DEV-2026-0002",
                ClientId = client.Id,
                DateEmission = DateTime.UtcNow,
                DateValidite = DateTime.UtcNow.AddDays(30),
                Statut = DevisStatus.Brouillon,
                CreatedAt = DateTime.UtcNow,
                Notes = "Ancienne note"
            };
            _context.Devis.Add(devis);
            await _context.SaveChangesAsync();

            var tracked = await _repository.GetByIdAsync(devis.Id);
            tracked!.Notes = "Nouvelle note";
            tracked.ProjetId = 42;

            await _repository.UpdateAsync(tracked);

            var reloaded = await _context.Devis.FindAsync(devis.Id);
            Assert.Equal("Nouvelle note", reloaded!.Notes);
            Assert.Equal(42, reloaded.ProjetId);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
