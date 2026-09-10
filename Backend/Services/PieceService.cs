using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

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
                piece.Reference = await GenerateReferenceAsync(piece);
            }
            else
            {
                piece.Reference = piece.Reference.Trim().ToUpperInvariant();
                if (!ReferencePattern.IsMatch(piece.Reference))
                {
                    throw new ArgumentException("La référence doit suivre le format XXX-000 ou VAS-XXX-000 (ex: MEC-001, VAS-CYL-001).");
                }
                await EnsureReferenceUniqueAsync(piece.Reference, excludeId: null);
            }

            piece.DateCreation = DateTime.Now;
            piece.Statut = PieceStatus.Brouillon;
            ClearFicheProduitFields(piece);

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

            if (string.IsNullOrWhiteSpace(piece.Reference))
            {
                piece.Reference = existingPiece.Reference;
            }
            else
            {
                piece.Reference = piece.Reference.Trim().ToUpperInvariant();
                if (piece.Reference != existingPiece.Reference)
                {
                    if (!ReferencePattern.IsMatch(piece.Reference))
                    {
                        throw new ArgumentException("La référence doit suivre le format XXX-000 ou VAS-XXX-000 (ex: MEC-001, VAS-CYL-001).");
                    }
                    await EnsureReferenceUniqueAsync(piece.Reference, excludeId: id);
                }
            }

            if (!CanEditFicheProduit(existingPiece.Statut))
            {
                PreserveFicheProduitFields(existingPiece, piece);
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
                    existingPiece.PrixVente.ToString("F2", CultureInfo.InvariantCulture),
                    piece.PrixVente.ToString("F2", CultureInfo.InvariantCulture)
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

            var oldStatut = Enum.Parse<PieceStatus>(existing.Statut.ToString());
            if (!IsValidTransition(oldStatut, nouveauStatut))
                throw new InvalidOperationException($"Transition impossible de {oldStatut} vers {nouveauStatut}");
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
            return await AnalyzeAndSaveStlStreamAsync(pieceId, stream, file.FileName, piece.Materiau.ToString());
        }

        public async Task<STLMetadata?> AnalyzeAndSaveStlFileAsync(int pieceId, string filePath, string fileName)
        {
            var piece = await _pieceRepository.GetByIdAsync(pieceId);
            if (piece == null) return null;

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await AnalyzeAndSaveStlStreamAsync(pieceId, stream, fileName, piece.Materiau.ToString());
        }

        private async Task<STLMetadata> AnalyzeAndSaveStlStreamAsync(int pieceId, Stream stream, string fileName, string? materiau)
        {
            var metadata = await _stlAnalyzerService.AnalyzeAsync(stream, fileName, pieceId, materiau);

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
        private bool IsValidTransition(PieceStatus oldStatus, PieceStatus newStatus)
        {
            // Si même statut, pas de changement
            if (oldStatus == newStatus) return true;

            // Liste des statuts dans l'ordre
            var order = new[] { PieceStatus.Brouillon, PieceStatus.Conception, PieceStatus.Prototypage,
                        PieceStatus.Validation, PieceStatus.Production, PieceStatus.Commercialisable };

            var oldIndex = Array.IndexOf(order, oldStatus);
            var newIndex = Array.IndexOf(order, newStatus);

            // On ne peut avancer que d'une étape (et pas reculer)
            return newIndex == oldIndex + 1;
        }

        private static bool CanEditFicheProduit(PieceStatus statut)
        {
            return statut is PieceStatus.Validation or PieceStatus.Production or PieceStatus.Commercialisable;
        }

        private static void ClearFicheProduitFields(Piece piece)
        {
            piece.Couleurs = null;
            piece.CapaciteContenance = null;
            piece.NormesCertifications = null;
            piece.InstructionsUtilisation = null;
            piece.PrecautionsUsage = null;
            piece.PublicCible = null;
            piece.Conditionnement = null;
            piece.DimensionsColis = null;
            piece.PoidsColisKg = null;
            piece.MoqUnites = null;
            piece.DelaiLivraisonJours = null;
            piece.PointsForts = null;
            piece.Faq = null;
            piece.TarifsDegressifs = null;
        }

        private static void PreserveFicheProduitFields(Piece source, Piece target)
        {
            target.Couleurs = source.Couleurs;
            target.CapaciteContenance = source.CapaciteContenance;
            target.NormesCertifications = source.NormesCertifications;
            target.InstructionsUtilisation = source.InstructionsUtilisation;
            target.PrecautionsUsage = source.PrecautionsUsage;
            target.PublicCible = source.PublicCible;
            target.Conditionnement = source.Conditionnement;
            target.DimensionsColis = source.DimensionsColis;
            target.PoidsColisKg = source.PoidsColisKg;
            target.MoqUnites = source.MoqUnites;
            target.DelaiLivraisonJours = source.DelaiLivraisonJours;
            target.PointsForts = source.PointsForts;
            target.Faq = source.Faq;
            target.TarifsDegressifs = source.TarifsDegressifs;
        }

        // Formats acceptés : l'ancien CAT-000 et la nomenclature vase VAS-FOR-000.
        private static readonly Regex ReferencePattern = new(@"^[A-Z]{3}(-[A-Z]{3})?-\d{3}$", RegexOptions.Compiled);

        private static string GetCategoriePrefix(PieceCategorie categorie) => categorie switch
        {
            PieceCategorie.Mecanique => "MEC",
            PieceCategorie.Electronique => "ELE",
            PieceCategorie.Decoration => "DEC",
            PieceCategorie.Outillage => "OUT",
            _ => "GEN"
        };

        private static string GetFormeVasePrefix(PieceFormeVase? forme) => forme switch
        {
            PieceFormeVase.Conique => "CON",
            PieceFormeVase.Organique => "ORG",
            PieceFormeVase.Twiste => "TWI",
            PieceFormeVase.Geometrique => "GEO",
            PieceFormeVase.Minimaliste => "MIN",
            PieceFormeVase.Moderne => "MOD",
            PieceFormeVase.Classique => "CLA",
            _ => "CYL"
        };

        private static string GetReferencePrefix(Piece piece)
        {
            if (piece.Categorie == PieceCategorie.Decoration)
            {
                piece.FormeVase ??= PieceFormeVase.Cylindrique;
                return $"VAS-{GetFormeVasePrefix(piece.FormeVase)}";
            }

            return GetCategoriePrefix(piece.Categorie);
        }

        private async Task<string> GenerateReferenceAsync(Piece piece)
        {
            var prefix = GetReferencePrefix(piece);
            var existing = await _pieceRepository.GetAllAsync() ?? Enumerable.Empty<Piece>();

            var maxSeq = existing
                .Where(p => !string.IsNullOrEmpty(p.Reference) && p.Reference.StartsWith(prefix + "-"))
                .Select(p => int.TryParse(p.Reference.Substring(prefix.Length + 1), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}-{(maxSeq + 1):D3}";
        }

        private async Task EnsureReferenceUniqueAsync(string reference, int? excludeId)
        {
            var existing = await _pieceRepository.GetAllAsync() ?? Enumerable.Empty<Piece>();
            var duplicate = existing.Any(p => p.Reference == reference && p.Id != excludeId);
            if (duplicate)
            {
                throw new ArgumentException($"La référence '{reference}' est déjà utilisée.");
            }
        }
    }
}
