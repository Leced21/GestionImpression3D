using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ClientMagicLinkRepository : IClientMagicLinkRepository
    {
        private readonly AppDbContext _context;
        public ClientMagicLinkRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClientMagicLink> CreateAsync(ClientMagicLink link)
        {
            _context.ClientMagicLinks.Add(link);
            await _context.SaveChangesAsync();
            return link;
        }

        public async Task<ClientMagicLink?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.ClientMagicLinks
                        .Include(l => l.Client)
                        .FirstOrDefaultAsync(l => l.TokenHash == tokenHash);
        }

        public async Task<ClientMagicLink> UpdateAsync(ClientMagicLink link)
        {
            await _context.SaveChangesAsync();
            return link;
        }
    }
}
