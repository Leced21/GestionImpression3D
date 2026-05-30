using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> CountActiveAdminsAsync()
        {
            // .AsNoTracking() améliore les performances pour les requêtes en lecture seule / agrégation
            return await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.Role == "Admin" && u.IsActive);
        }

        public async Task<int> CountAdminsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.Role == "Admin");
        }

        public async Task<User> CreateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            await _context.Users.AddAsync(user); // Utilisation de AddAsync pour le pattern asynchrone complet
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking() // Évite de surcharger la mémoire avec le tracking d'entités qu'on veut juste lister
                .OrderBy(u => u.Id)
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            return await _context.Users
                .AsNoTracking() // Recommandé pour le Login (lecture seule)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Approche plus sûre que EntityState.Modified qui gère mieux le tracking 
            // et ne met à jour que ce qui a changé si l'entité est déjà attachée.
            _context.Users.Update(user);

            await _context.SaveChangesAsync();
            return user;
        }
    }
}