using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class PieceService : IPieceService
    {
        private readonly IPieceRepository _pieceRepository;
        private readonly IAuditLogger _auditLogger;
        private readonly IPieceVersionRepository _pieceVersionRepository;
        private readonly ISTLAnalyzerService _stlAnalyzerService;
        private readonly IServiceProvider _serviceProvider;
        public PieceService(IPieceRepository pieceRepository, IAuditLogger auditLogger, IPieceVersionRepository pieceVersionRepository, ISTLAnalyzerService stlAnalyzerService, IServiceProvider serviceProvider)
        {
            _pieceRepository = pieceRepository;
            _auditLogger = auditLogger;
            _pieceVersionRepository = pieceVersionRepository;
            _stlAnalyzerService = stlAnalyzerService;
            _serviceProvider = serviceProvider;
        }
        public async Task<decimal> CalculerPrixRecommandéAsync(int id)
        {
            return await _pieceRepository.CalculerPrixRecommandéAsync(id);
        }

        public async Task<Piece> CreateAsync(Piece piece)
        {
            if (string.IsNullOrWhiteSpace(piece.Reference))
            {
                piece.Reference = $"P-{DateTime.Now.Ticks}";
            }

            piece.DateCreation = DateTime.Now;
            piece.Statut = PieceStatus.Brouillon;

            var created = await _pieceRepository.CreateAsync(piece);

            // Créer la version initiale directement via le repository
            var nextVersion = await _pieceVersionRepository.GetNextVersionNumberAsync(created.Id);

            var version = new PieceVersion
            {
                PieceId = created.Id,
                VersionNumber = nextVersion,
                Nom = created.Nom,
                Description = created.Description,
                CoutMatiere = created.CoutMatiere,
                CoutMachine = created.CoutMachine,
                CoutMainOeuvre = created.CoutMainOeuvre,
                PrixVente = created.PrixVente,
                StlFileName = created.StlFileName,
                ChangeLog = "Version initiale",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsPrototype = true
            };

            await _pieceVersionRepository.CreateAsync(version);

            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _pieceRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var entityName = existing.Nom;

            await _pieceRepository.DeleteAsync(id);

            await _auditLogger.LogDeletionAsync(
                EntityType.Piece, id, entityName
            );

            return true;
        }

        public async Task<IEnumerable<Piece>> GetAllAsync()
        {
            return await _pieceRepository.GetAllAsync();
        }

        public async Task<Piece?> GetByIdAsync(int id)
        {
            return await _pieceRepository.GetByIdAsync(id);
        }

        public async Task<Piece?> UpdateAsync(int id, Piece piece)
        {
            var existingPiece = await _pieceRepository.GetByIdAsync(id);
            if (existingPiece == null)
            {
                return null;
            }
            if (existingPiece.Nom != piece.Nom)
            {
                await _auditLogger.LogUpdateAsync(
                    EntityType.Piece, id, "Nom", existingPiece.Nom, piece.Nom
                );
            }

            if (existingPiece.PrixVente != piece.PrixVente)
            {
                await _auditLogger.LogUpdateAsync(
                    EntityType.Piece, id, "PrixVente",
                    existingPiece.PrixVente.ToString("F2"),
                    piece.PrixVente.ToString("F2")
                );
            }

            if (existingPiece.Description != piece.Description)
            {
                await _auditLogger.LogUpdateAsync(
                    EntityType.Piece, id, "Description",
                    existingPiece.Description ?? "",
                    piece.Description ?? ""
                );
            }

            piece.Id = id;
            piece.DateCreation = existingPiece.DateCreation;

            return await _pieceRepository.UpdateAsync(id, piece);
        }

        public async Task<Piece?> UpdateStatutAsync(int id, PieceStatus nouveauStatut)
        {
            var existing = await _pieceRepository.GetByIdAsync(id);
            if (existing == null) return null;

            var oldStatut = existing.Statut;
            existing.Statut = nouveauStatut;
            await _pieceRepository.UpdateStatutAsync(id, nouveauStatut);
            await _auditLogger.LogStatusChangeAsync(EntityType.Piece, id, oldStatut.ToString(), nouveauStatut.ToString());
            return existing;
        }
        public async Task<STLMetadata?> AnalyzeSTLAsync(int pieceId, IFormFile file)
        {
            var piece = await _pieceRepository.GetByIdAsync(pieceId);
            if (piece == null) return null;

            using var stream = file.OpenReadStream();
            var metadata = await _stlAnalyzerService.AnalyzeAsync(stream, file.FileName, pieceId);

            // Sauvegarder les métadonnées
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existing = await context.STLMetadata.FirstOrDefaultAsync(m => m.PieceId == pieceId);
            if (existing != null)
            {
                context.STLMetadata.Remove(existing);
            }

            context.STLMetadata.Add(metadata);
            await context.SaveChangesAsync();

            return metadata;
        }
    }
}
