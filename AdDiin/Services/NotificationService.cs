using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface INotificationService
    {
        Task<UserNotification> CreateNotificationAsync(int userId, string title, string message, string category = "general", string? linkUrl = null);
        Task<List<UserNotification>> GetUserNotificationsAsync(int userId, string category = "all", int limit = 50);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task SeedDefaultRemindersAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserNotification> CreateNotificationAsync(int userId, string title, string message, string category = "general", string? linkUrl = null)
        {
            var notification = new UserNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Category = category,
                LinkUrl = linkUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserNotifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<UserNotification>> GetUserNotificationsAsync(int userId, string category = "all", int limit = 50)
        {
            var query = _context.UserNotifications
                .Where(n => n.UserId == userId);

            if (!string.IsNullOrEmpty(category) && category.ToLower() != "all")
            {
                query = query.Where(n => n.Category.ToLower() == category.ToLower());
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var item = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (item == null) return false;

            item.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!unread.Any()) return true;

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SeedDefaultRemindersAsync(int userId)
        {
            var hasAny = await _context.UserNotifications.AnyAsync(n => n.UserId == userId);
            if (!hasAny)
            {
                var defaults = new List<UserNotification>
                {
                    new()
                    {
                        UserId = userId,
                        Title = "Welcome to ADDiin Platform",
                        Message = "Your digital companion for daily Islamic growth. Start by completing your Daily Goals in My Deen.",
                        Category = "general",
                        LinkUrl = "/my-deen",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddHours(-2)
                    },
                    new()
                    {
                        UserId = userId,
                        Title = "Daily Dhikr & Quran Target Reminder",
                        Message = "Remember to recite your morning/evening Adhkar and read your daily Quran portion.",
                        Category = "mydeen",
                        LinkUrl = "/my-deen",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    },
                    new()
                    {
                        UserId = userId,
                        Title = "Next Prayer Alert",
                        Message = "Stay connected with Allah through timely prayers. Check today's full prayer schedule.",
                        Category = "prayer",
                        LinkUrl = "/prayer-times",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                    }
                };

                _context.UserNotifications.AddRange(defaults);
                await _context.SaveChangesAsync();
            }
        }
    }
}
