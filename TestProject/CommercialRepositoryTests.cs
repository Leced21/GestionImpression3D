using Backend.Data;
using Backend.Enums;
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

        [Fact]
        public async Task RestoreStockAsync_AddsQuantityBackToPieceStock()
        {
            var piece = new Piece { Nom = "Support moteur", Reference = "REF-001", Stock = 5 };
            _context.Pieces.Add(piece);
            await _context.SaveChangesAsync();

            var result = await _repository.RestoreStockAsync(piece.Id, 3);

            Assert.True(result);
            var reloaded = await _context.Pieces.FindAsync(piece.Id);
            Assert.Equal(8, reloaded!.Stock);
        }

        [Fact]
        public async Task RestoreStockAsync_WithUnknownPiece_ReturnsFalse()
        {
            var result = await _repository.RestoreStockAsync(999999, 3);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateStatutAsync_ToLivree_SetsDateLivraisonAndPersistsEnum()
        {
            var commande = new Commande { NumeroCommande = "CMD-2026-0010", DateCommande = DateTime.Now };
            _context.Commandes.Add(commande);
            await _context.SaveChangesAsync();

            var updated = await _repository.UpdateStatutAsync(commande.Id, CommandeStatus.Livrée);

            Assert.NotNull(updated);
            Assert.Equal(CommandeStatus.Livrée, updated!.Statut);
            Assert.NotNull(updated.DateLivraison);

            var reloaded = await _context.Commandes.FindAsync(commande.Id);
            Assert.Equal(CommandeStatus.Livrée, reloaded!.Statut);
        }

        [Fact]
        public async Task GetChiffreAffairesAsync_OnlySumsLivreeCommandes()
        {
            _context.Commandes.AddRange(
                new Commande { NumeroCommande = "CMD-A", DateCommande = DateTime.Now, Statut = CommandeStatus.Livrée, Total = 100m },
                new Commande { NumeroCommande = "CMD-B", DateCommande = DateTime.Now, Statut = CommandeStatus.EnAttente, Total = 999m }
            );
            await _context.SaveChangesAsync();

            var ca = await _repository.GetChiffreAffairesAsync();

            Assert.Equal(100m, ca);
        }

        [Fact]
        public async Task GetStatistiquesCommandesAsync_GroupsByEnumStatut()
        {
            _context.Commandes.AddRange(
                new Commande { NumeroCommande = "CMD-A", DateCommande = DateTime.Now, Statut = CommandeStatus.EnAttente },
                new Commande { NumeroCommande = "CMD-B", DateCommande = DateTime.Now, Statut = CommandeStatus.EnAttente },
                new Commande { NumeroCommande = "CMD-C", DateCommande = DateTime.Now, Statut = CommandeStatus.Livrée }
            );
            await _context.SaveChangesAsync();

            var stats = await _repository.GetStatistiquesCommandesAsync();

            Assert.Equal(2, stats[CommandeStatus.EnAttente]);
            Assert.Equal(1, stats[CommandeStatus.Livrée]);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
