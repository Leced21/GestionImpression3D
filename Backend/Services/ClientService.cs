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
            var existing = await _clientRepository.GetByEmailAsync(request.Email);
            if (existing != null)
                throw new InvalidOperationException("Un client avec cet email existe déjà");

            var client = new Client
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

            client.Nom = request.Nom;
            client.Email = request.Email;
            client.Telephone = request.Telephone;
            client.Adresse = request.Adresse;
            client.CodePostal = request.CodePostal;
            client.Ville = request.Ville;
            client.Pays = request.Pays;
            client.Siret = request.Siret;
            client.TVAIntra = request.TVAIntra;
            client.Notes = request.Notes;
            client.IsActive = request.IsActive;

            var updated = await _clientRepository.UpdateAsync(client);

            await _auditLogger.LogUpdateAsync(EntityType.Client, id, "Client", "Modifié", updated.Nom);

            return updated;
        }
    }
}
