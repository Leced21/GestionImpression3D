using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class ProjetService : IProjetService
    {
        private readonly IProjetRepository _projetRepository;
        private readonly IPieceRepository _pieceRepository;

        public ProjetService(IProjetRepository projetRepository, IPieceRepository pieceRepository)
        {
            _projetRepository = projetRepository;
            _pieceRepository = pieceRepository;
        }
        public async Task<ProjetPiece> AjouterPieceAsync(int projetId, int pieceId, int quantite)
        {
            var piece = await _pieceRepository.GetByIdAsync(pieceId);
            if (piece == null)
                throw new InvalidOperationException("Pièce non trouvée");
            return await _projetRepository.AddPieceAsync(projetId, pieceId, quantite);
        }

        public async Task<Projet> CreateAsync(Projet projet)
        {
            return await _projetRepository.CreateAsync(projet);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _projetRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Projet>> GetAllAsync()
        {
            return await _projetRepository.GetAllAsync();
        }

        public async Task<Projet?> GetByIdAsync(int id)
        {
            return await _projetRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ProjetPiece>> GetPiecesAsync(int projetId)
        {
            return await _projetRepository.GetPiecesByProjetAsync(projetId);
        }

        public async Task<object> GetStatsAsync(int id)
        {
            var projet = await _projetRepository.GetByIdAsync(id);
            if (projet == null) return new { };

            var pieces = projet.ProjetPieces;
            var coutTotal = pieces.Sum(p => (p.Piece?.CoutTotal ?? 0) * p.Quantite);
            var prixTotal = pieces.Sum(p => (p.Piece?.PrixVente ?? 0) * p.Quantite);

            return new
            {
                NombrePieces = pieces.Count,
                QuantiteTotale = pieces.Sum(p => p.Quantite),
                CoutTotal = coutTotal,
                PrixTotal = prixTotal,
                Marge = prixTotal - coutTotal
            };
        }

        public async Task<bool> RetirerPieceAsync(int projetId, int pieceId)
        {
            return await _projetRepository.RemovePieceAsync(projetId, pieceId);
        }

        public async Task<Projet?> UpdateAsync(int id, Projet projet)
        {
            return await _projetRepository.UpdateAsync(id, projet);
        }
    }
}
