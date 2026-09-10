using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class DevisService : IDevisService
    {
        private readonly IDevisRepository _devisRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IPieceRepository _pieceRepository;
        private readonly IOrdreFabricationService _ordreFabricationService;
        private readonly IFactureService _factureService;
        private readonly IPdfExportService _pdfExportService;
        private readonly IAuditLogger _auditLogger;
        public DevisService(IDevisRepository devisRepository, IClientRepository clientRepository, IPieceRepository pieceRepository, IOrdreFabricationService ordreFabricationService, IFactureService factureService, IPdfExportService pdfExportService, IAuditLogger auditLogger)
        {
            _devisRepository = devisRepository;
            _clientRepository = clientRepository;
            _pieceRepository = pieceRepository;
            _ordreFabricationService = ordreFabricationService;
            _factureService = factureService;
            _pdfExportService = pdfExportService;
            _auditLogger = auditLogger;
        }
        public async Task<Devis> CreateAsync(CreateDevisRequest request)
        {
            ValidateDevisRequest(request);

            var client = await _clientRepository.GetByIdAsync(request.ClientId);
            if (client == null)
                throw new InvalidOperationException("Client non trouvé");

            var lignes = new List<DevisLigne>();
            decimal totalHT = 0;

            foreach (var ligneReq in request.Lignes)
            {
                decimal prixUnitaire = ligneReq.PrixUnitaire;
                string description = ligneReq.Description;

                if (ligneReq.PieceId.HasValue)
                {
                    var piece = await _pieceRepository.GetByIdAsync(ligneReq.PieceId.Value);
                    if (piece != null)
                    {
                        prixUnitaire = piece.PrixVente;
                        description = piece.Nom;
                    }
                }

                var ligne = new DevisLigne
                {
                    PieceId = ligneReq.PieceId,
                    Description = description,
                    Quantite = ligneReq.Quantite,
                    PrixUnitaire = prixUnitaire
                };
                lignes.Add(ligne);
                totalHT += ligne.Total;
            }

            var totalTTC = totalHT * (1 + request.TVA / 100);

            var devis = new Devis
            {
                ClientId = request.ClientId,
                ProjetId = request.ProjetId,
                DateEmission = DateTime.UtcNow,
                DateValidite = request.DateValidite,
                TotalHT = totalHT,
                TVA = request.TVA,
                TotalTTC = totalTTC,
                Statut = DevisStatus.Brouillon,
                Notes = request.Notes,
                Conditions = request.Conditions,
                Lignes = lignes
            };

            var created = await _devisRepository.CreateAsync(devis);

            await _auditLogger.LogCreationAsync(EntityType.Devis, created.Id, created.NumeroDevis);

            return created;
        }

        public async Task<Devis?> UpdateAsync(int id, UpdateDevisRequest request)
        {
            var existing = await _devisRepository.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.Statut == DevisStatus.Accepté)
                throw new InvalidOperationException("Un devis accepté ne peut plus être modifié.");

            ValidateDevisRequest(request);

            var client = await _clientRepository.GetByIdAsync(request.ClientId);
            if (client == null)
                throw new InvalidOperationException("Client non trouvé");

            var lignes = new List<DevisLigne>();
            decimal totalHT = 0;

            foreach (var ligneReq in request.Lignes)
            {
                decimal prixUnitaire = ligneReq.PrixUnitaire;
                string description = ligneReq.Description;

                if (ligneReq.PieceId.HasValue)
                {
                    var piece = await _pieceRepository.GetByIdAsync(ligneReq.PieceId.Value);
                    if (piece != null)
                    {
                        prixUnitaire = piece.PrixVente;
                        description = piece.Nom;
                    }
                }

                var ligne = new DevisLigne
                {
                    PieceId = ligneReq.PieceId,
                    Description = description,
                    Quantite = ligneReq.Quantite,
                    PrixUnitaire = prixUnitaire
                };
                lignes.Add(ligne);
                totalHT += ligne.Total;
            }

            existing.ClientId = request.ClientId;
            existing.ProjetId = request.ProjetId;
            existing.DateValidite = request.DateValidite;
            existing.TVA = request.TVA;
            existing.TotalHT = totalHT;
            existing.TotalTTC = totalHT * (1 + request.TVA / 100);
            existing.Notes = request.Notes;
            existing.Conditions = request.Conditions;
            existing.Lignes = lignes;

            var updated = await _devisRepository.UpdateAsync(existing);

            await _auditLogger.LogUpdateAsync(EntityType.Devis, id, "Devis", "Modifié", updated.NumeroDevis);

            return updated;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var devis = await _devisRepository.GetByIdAsync(id);
            if (devis == null) return false;

            var result = await _devisRepository.DeleteAsync(id);

            if (result)
                await _auditLogger.LogDeletionAsync(EntityType.Devis, id, devis.NumeroDevis);

            return result;
        }

        public async Task<byte[]> GeneratePdfAsync(int id)
        {
            var devis = await GetByIdAsync(id);
            if (devis == null) return Array.Empty<byte>();

            return await _pdfExportService.ExportDevisPdfAsync(devis);
        }

        public async Task<IEnumerable<Devis>> GetAllAsync()
        {
            return await _devisRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Devis>> GetByClientAsync(int clientId)
        {
            return await _devisRepository.GetByClientAsync(clientId);
        }

        public async Task<Devis?> GetByIdAsync(int id)
        {
            return await _devisRepository.GetByIdAsync(id);
        }

        public async Task<DevisStatisticsDto> GetStatisticsAsync()
        {
            return await _devisRepository.GetStatisticsAsync();
        }

        public async Task<Devis?> UpdateStatutAsync(int id, DevisStatus statut)
        {
            var devis = await _devisRepository.GetByIdAsync(id);
            if (devis == null) return null;

            var oldStatut = devis.Statut;
            var isNewlyAccepted = statut == DevisStatus.Accepté && oldStatut != DevisStatus.Accepté;

            if (isNewlyAccepted && !devis.ProjetId.HasValue)
                throw new InvalidOperationException("Un projet doit être associé au devis avant de pouvoir l'accepter.");

            var updated = await _devisRepository.UpdateStatutAsync(id, statut);

            await _auditLogger.LogStatusChangeAsync(EntityType.Devis, id, oldStatut.ToString(), statut.ToString());

            if (isNewlyAccepted && updated != null)
            {
                await GenerateOrdresFabricationAsync(updated);
                await GenerateFactureAsync(updated);
            }

            return updated;
        }

        private async Task GenerateOrdresFabricationAsync(Devis devis)
        {
            // Idempotence : évite de régénérer des ordres si le statut "Accepté" est renvoyé plusieurs fois.
            if (await _ordreFabricationService.ExistsForDevisAsync(devis.Id))
                return;

            foreach (var ligne in devis.Lignes.Where(l => l.PieceId.HasValue))
            {
                await _ordreFabricationService.CreateAsync(new CreateOrdreRequest
                {
                    ProjetId = devis.ProjetId!.Value,
                    PieceId = ligne.PieceId!.Value,
                    DevisId = devis.Id,
                    Quantite = ligne.Quantite,
                    Notes = $"Généré automatiquement depuis le devis {devis.NumeroDevis}"
                });
            }
        }

        private async Task GenerateFactureAsync(Devis devis)
        {
            // Idempotence : évite de régénérer une facture si le statut "Accepté" est renvoyé plusieurs fois.
            if (await _factureService.ExistsForDevisAsync(devis.Id))
                return;

            await _factureService.CreateFromDevisAsync(devis);
        }

        private static void ValidateDevisRequest(CreateDevisRequest request)
        {
            ValidateDevisValues(request.ClientId, request.DateValidite, request.TVA, request.Lignes);
        }

        private static void ValidateDevisRequest(UpdateDevisRequest request)
        {
            ValidateDevisValues(request.ClientId, request.DateValidite, request.TVA, request.Lignes);
        }

        private static void ValidateDevisValues(int clientId, DateTime dateValidite, decimal tva, IReadOnlyCollection<DevisLigneRequest> lignes)
        {
            if (clientId <= 0)
                throw new InvalidOperationException("Le client est obligatoire.");
            if (dateValidite == default)
                throw new InvalidOperationException("La date de validité est obligatoire.");
            if (tva < 0 || tva > 100)
                throw new InvalidOperationException("La TVA doit être comprise entre 0 et 100.");
            if (lignes.Count == 0)
                throw new InvalidOperationException("Un devis doit contenir au moins une ligne.");

            foreach (var ligne in lignes)
            {
                if (ligne.PieceId.HasValue && ligne.PieceId.Value <= 0)
                    throw new InvalidOperationException("L'identifiant de pièce est invalide.");
                if (!ligne.PieceId.HasValue && string.IsNullOrWhiteSpace(ligne.Description))
                    throw new InvalidOperationException("La description est obligatoire pour une ligne sans pièce.");
                if (ligne.Quantite <= 0)
                    throw new InvalidOperationException("La quantité doit être positive.");
                if (ligne.PrixUnitaire < 0)
                    throw new InvalidOperationException("Le prix unitaire ne peut pas être négatif.");

                ligne.Description = ligne.Description.Trim();
            }
        }
    }
}
