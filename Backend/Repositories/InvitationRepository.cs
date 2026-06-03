using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly AppDbContext _context;
        public InvitationRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Invitation> CreateAsync(Invitation invitation)
        {
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();
            return invitation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invitation = await GetByIdAsync(id);
            if (invitation == null) return false;

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Invitation?> GetByIdAsync(int id)
        {
            return await _context.Invitations.FindAsync(id);
        }

        public async Task<Invitation?> GetByTokenAsync(string token)
        {
            return await _context.Invitations
                .FirstOrDefaultAsync(i => i.Token == token);
        }

        public async Task<IEnumerable<Invitation>> GetPendingAsync()
        {
            return await _context.Invitations
            .Where(i => !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            return await _context.Invitations
                .AnyAsync(i => i.Token == token && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<Invitation> UpdateAsync(Invitation invitation)
        {
            _context.Entry(invitation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return invitation;
        }
    }
}
