using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IActivityService
    {
        Task<List<Activity>> GetActiveActivitiesAsync();
        Task<List<Activity>> GetAllActivitiesAsync();
        Task<Activity?> GetByIdAsync(int id);
        Task<Activity> CreateAsync(Activity activity);
        Task<Activity?> UpdateAsync(int id, Activity updated);
        Task<bool> DeleteAsync(int id);
    }

    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;

        public ActivityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Activity>> GetActiveActivitiesAsync()
        {
            return await _context.Activities
                .Where(a => a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Activity>> GetAllActivitiesAsync()
        {
            return await _context.Activities
                .OrderBy(a => a.DisplayOrder)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _context.Activities.FindAsync(id);
        }

        public async Task<Activity> CreateAsync(Activity activity)
        {
            activity.CreatedAt = DateTime.UtcNow;
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            return activity;
        }

        public async Task<Activity?> UpdateAsync(int id, Activity updated)
        {
            var item = await _context.Activities.FindAsync(id);
            if (item == null) return null;

            item.Title = updated.Title;
            item.Description = updated.Description;
            if (!string.IsNullOrEmpty(updated.ImageUrl)) item.ImageUrl = updated.ImageUrl;
            item.Category = updated.Category;
            item.IsActive = updated.IsActive;
            item.DisplayOrder = updated.DisplayOrder;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Activities.FindAsync(id);
            if (item == null) return false;

            _context.Activities.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
