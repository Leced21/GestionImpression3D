using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PieceService : IPieceService
    {
        private readonly IPieceRepository _pieceRepository;
        public PieceService(IPieceRepository pieceRepository)
        {
            _pieceRepository = pieceRepository;
        }
        public async Task<decimal> CalculerPrixRecommandéAsync(int id)
        {
            return await _pieceRepository.CalculerPrixRecommandéAsync(id);
        }

        public async Task<Piece> CreateAsync(Piece piece)
        {
            return await _pieceRepository.CreateAsync(piece);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _pieceRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Piece>> GetAllAsync()
        {
            return await _pieceRepository.GetAllAsync();
        }

        public async Task<Piece?> GetByIdAsync(int id)
        {
            return await _pieceRepository.GetByIdAsync(id);
        }

        public async Task<Piece?> UpdateStatutAsync(int id, string nouveauStatut)
        {
            return await _pieceRepository.UpdateStatutAsync(id, nouveauStatut);
        }
    }
}
