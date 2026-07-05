using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAuditLogger _auditLogger;
        public ClientService(IClientRepository clientRepository, IAuditLogger auditLogger)
        {
            _clientRepository = clientRepository;
            _auditLogger = auditLogger;
        }
        public async Task<Client> CreateAsync(CreateClientRequest request)
        {
            Normalize(request);

            if (string.IsNullOrWhiteSpace(request.Nom))
                throw new InvalidOperationException("Le nom du client est obligatoire");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("L'email du client est obligatoire");

            var existing = await _clientRepository.GetByEmailAsync(request.Email);
            if (existing != null)
                throw new InvalidOperationException("Un client avec cet email existe déjà");

            var client = ToClient(request);

            var created = await _clientRepository.CreateAsync(client);

            await _auditLogger.LogCreationAsync(EntityType.Client, created.Id, created.Nom);

            return created;
        }

        public async Task<Client?> EnsureClientAsync(CreateClientRequest request)
        {
            Normalize(request);

            if (string.IsNullOrWhiteSpace(request.Nom) && string.IsNullOrWhiteSpace(request.Email))
                return null;

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("L'email du client est obligatoire pour centraliser sa fiche");

            var existing = await _clientRepository.GetByEmailAsync(request.Email);
            if (existing != null)
            {
                var changed = false;

                if (!string.IsNullOrWhiteSpace(request.Nom) && existing.Nom != request.Nom)
                {
                    existing.Nom = request.Nom;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(request.Telephone) && existing.Telephone != request.Telephone)
                {
                    existing.Telephone = request.Telephone;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(request.Adresse) && existing.Adresse != request.Adresse)
                {
                    existing.Adresse = request.Adresse;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(request.CodePostal) && existing.CodePostal != request.CodePostal)
                {
                    existing.CodePostal = request.CodePostal;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(request.Ville) && existing.Ville != request.Ville)
                {
                    existing.Ville = request.Ville;
                    changed = true;
                }

                if (changed)
                {
                    var updated = await _clientRepository.UpdateAsync(existing);
                    await _auditLogger.LogUpdateAsync(EntityType.Client, updated.Id, "Client", "Synchronisé", updated.Nom);
                    return updated;
                }

                return existing;
            }

            if (string.IsNullOrWhiteSpace(request.Nom))
                request.Nom = request.Email;

            var client = ToClient(request);
            var created = await _clientRepository.CreateAsync(client);
            await _auditLogger.LogCreationAsync(EntityType.Client, created.Id, created.Nom);
            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return false;

            var result = await _clientRepository.DeleteAsync(id);

            if (result)
                await _auditLogger.LogDeletionAsync(EntityType.Client, id, client.Nom);

            return result;
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _clientRepository.GetAllAsync();
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            return await _clientRepository.GetByEmailAsync(email);
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _clientRepository.GetByIdAsync(id);
        }

        public async Task<int> GetCountAsync()
        {
            return await _clientRepository.GetCountAsync();
        }

        public async Task<IEnumerable<Client>> SearchAsync(string searchTerm)
        {
            return await _clientRepository.SearchAsync(searchTerm);
        }

        public async Task<Client?> UpdateAsync(int id, UpdateClientRequest request)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return null;

            client.Nom = request.Nom.Trim();
            client.Email = request.Email.Trim().ToLowerInvariant();
            client.Telephone = request.Telephone?.Trim() ?? string.Empty;
            client.Adresse = request.Adresse?.Trim() ?? string.Empty;
            client.CodePostal = request.CodePostal?.Trim() ?? string.Empty;
            client.Ville = request.Ville?.Trim() ?? string.Empty;
            client.Pays = string.IsNullOrWhiteSpace(request.Pays) ? "France" : request.Pays.Trim();
            client.Siret = string.IsNullOrWhiteSpace(request.Siret) ? null : request.Siret.Trim();
            client.TVAIntra = string.IsNullOrWhiteSpace(request.TVAIntra) ? null : request.TVAIntra.Trim();
            client.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            client.IsActive = request.IsActive;

            var updated = await _clientRepository.UpdateAsync(client);

            await _auditLogger.LogUpdateAsync(EntityType.Client, id, "Client", "Modifié", updated.Nom);

            return updated;
        }

        private static Client ToClient(CreateClientRequest request)
        {
            return new Client
            {
                Nom = request.Nom,
                Email = request.Email,
                Telephone = request.Telephone,
                Adresse = request.Adresse,
                CodePostal = request.CodePostal,
                Ville = request.Ville,
                Pays = request.Pays,
                Siret = request.Siret,
                TVAIntra = request.TVAIntra,
                Notes = request.Notes,
                IsActive = true
            };
        }

        private static void Normalize(CreateClientRequest request)
        {
            request.Nom = request.Nom.Trim();
            request.Email = request.Email.Trim().ToLowerInvariant();
            request.Telephone = request.Telephone?.Trim() ?? string.Empty;
            request.Adresse = request.Adresse?.Trim() ?? string.Empty;
            request.CodePostal = request.CodePostal?.Trim() ?? string.Empty;
            request.Ville = request.Ville?.Trim() ?? string.Empty;
            request.Pays = string.IsNullOrWhiteSpace(request.Pays) ? "France" : request.Pays.Trim();
            request.Siret = string.IsNullOrWhiteSpace(request.Siret) ? null : request.Siret.Trim();
            request.TVAIntra = string.IsNullOrWhiteSpace(request.TVAIntra) ? null : request.TVAIntra.Trim();
            request.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }
    }
}
