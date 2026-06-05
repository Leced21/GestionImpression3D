using Backend.Models;
using Backend.Enums;

namespace Backend.Interface
{
    public interface IPieceService
    {
        Task<IEnumerable<Piece>> GetAllAsync();
        Task<Piece?> GetByIdAsync(int id);
        Task<Piece> CreateAsync(Piece piece);
        Task<Piece> UpdateAsync(int id, Piece piece);
        Task<Piece?> UpdateStatutAsync(int id, PieceStatus nouveauStatut);
        Task<decimal> CalculerPrixRecommandéAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
