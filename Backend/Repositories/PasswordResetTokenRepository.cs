using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;
        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.PasswordResetTokens
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task<PasswordResetToken> UpdateAsync(PasswordResetToken token)
        {
            await _context.SaveChangesAsync();
            return token;
        }
    }
}
