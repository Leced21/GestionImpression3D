using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;
        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Client> CreateAsync(Client client)
        {
            client.CreatedAt = DateTime.UtcNow;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = await GetByIdAsync(id);
            if (client == null) return false;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients
                        .OrderBy(c => c.Nom)
                        .ToListAsync();
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            return await _context.Clients
                        .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients
                        .Include(c => c.Devis)
                        .Include(c => c.Commandes)
                        .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Clients.CountAsync();
        }

        public async Task<IEnumerable<Client>> SearchAsync(string searchTerm)
        {
            return await _context.Clients
                        .Where(c => c.Nom.Contains(searchTerm) ||
                                    c.Email.Contains(searchTerm) ||
                                    c.Siret.Contains(searchTerm))
                        .ToListAsync();
        }

        public async Task<Client> UpdateAsync(Client client)
        {
            client.UpdatedAt = DateTime.UtcNow;
            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return client;
        }
    }
}
