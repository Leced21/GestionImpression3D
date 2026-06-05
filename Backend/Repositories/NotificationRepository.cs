using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<AppNotification> CreateAsync(AppNotification notification)
        {
            _context.AppNotifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteAllForUserAsync(int userId)
        {
            var notifications = await _context.AppNotifications
                    .Where(n => n.UserId == userId)
                    .ToListAsync();

            _context.AppNotifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await GetByIdAsync(id);
            if (notification == null) return false;

            _context.AppNotifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AppNotification?> GetByIdAsync(int id)
        {
            return await _context.AppNotifications.FindAsync(id);
        }

        public async Task<IEnumerable<AppNotification>> GetByUserAsync(int userId)
        {
            return await _context.AppNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<AppNotification>> GetUnreadByUserAsync(int userId)
        {
            return await _context.AppNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.AppNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var count = await _context.AppNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow));
            return count;
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notification = await GetByIdAsync(id);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AppNotification> UpdateAsync(AppNotification notification)
        {
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return notification;
        }
    }
}
