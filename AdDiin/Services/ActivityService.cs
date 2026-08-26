using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IActivityService
    {
        Task<List<Activity>> GetActiveActivitiesAsync(string? category = null, string? search = null);
        Task<List<Activity>> GetAllActivitiesAsync();
        Task<Activity?> GetByIdAsync(int id);
        Task<Activity> CreateAsync(Activity activity);
        Task<Activity?> UpdateAsync(int id, Activity updated);
        Task<bool> DeleteAsync(int id);

        // Program Registrations
        Task<ProgramRegistration> RegisterForProgramAsync(ProgramRegistrationInputModel model, int? userId);
        Task<List<ProgramRegistration>> GetUserRegistrationsAsync(int userId);
        Task<List<ProgramRegistration>> GetAllRegistrationsAsync(string? status = null, int? activityId = null, string? search = null);
        Task<ProgramRegistration?> GetRegistrationByIdAsync(int id);
        Task<bool> ReviewRegistrationAsync(int registrationId, string newStatus, string? adminRemarks);
    }

    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ActivityService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<Activity>> GetActiveActivitiesAsync(string? category = null, string? search = null)
        {
            var query = _context.Activities
                .Where(a => a.IsActive);

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(a => a.Title.ToLower().Contains(term) || 
                                         a.Description.ToLower().Contains(term) ||
                                         (a.Location != null && a.Location.ToLower().Contains(term)) ||
                                         (a.Organizer != null && a.Organizer.ToLower().Contains(term)));
            }

            return await query
                .OrderBy(a => a.DisplayOrder)
                .ThenByDescending(a => a.ProgramDate)
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
            return await _context.Activities
                .Include(a => a.Registrations)
                .FirstOrDefaultAsync(a => a.Id == id);
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
            item.Location = updated.Location;
            item.Organizer = updated.Organizer;
            item.Instructor = updated.Instructor;
            item.ProgramDate = updated.ProgramDate;
            item.StartTime = updated.StartTime;
            item.EndTime = updated.EndTime;
            item.MaxCapacity = updated.MaxCapacity;
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

        public async Task<ProgramRegistration> RegisterForProgramAsync(ProgramRegistrationInputModel model, int? userId)
        {
            var activity = await _context.Activities.FindAsync(model.ActivityId);

            var registration = new ProgramRegistration
            {
                ActivityId = model.ActivityId,
                UserId = userId,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Notes = model.Notes,
                Status = "Pending",
                RegisteredAt = DateTime.UtcNow
            };

            _context.ProgramRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            if (userId.HasValue)
            {
                await _notificationService.CreateNotificationAsync(
                    userId.Value,
                    "📋 Program Registration Submitted",
                    $"Your registration request for '{activity?.Title ?? "Islamic Program"}' has been received and is currently under review.",
                    "activities",
                    "/user-profile"
                );
            }

            return registration;
        }

        public async Task<List<ProgramRegistration>> GetUserRegistrationsAsync(int userId)
        {
            return await _context.ProgramRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();
        }

        public async Task<List<ProgramRegistration>> GetAllRegistrationsAsync(string? status = null, int? activityId = null, string? search = null)
        {
            var query = _context.ProgramRegistrations
                .Include(r => r.Activity)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status.ToLower() == status.ToLower());
            }

            if (activityId.HasValue && activityId.Value > 0)
            {
                query = query.Where(r => r.ActivityId == activityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(r => r.FullName.ToLower().Contains(term) ||
                                         r.Email.ToLower().Contains(term) ||
                                         r.PhoneNumber.Contains(term) ||
                                         (r.Activity != null && r.Activity.Title.ToLower().Contains(term)));
            }

            return await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();
        }

        public async Task<ProgramRegistration?> GetRegistrationByIdAsync(int id)
        {
            return await _context.ProgramRegistrations
                .Include(r => r.Activity)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> ReviewRegistrationAsync(int registrationId, string newStatus, string? adminRemarks)
        {
            var registration = await _context.ProgramRegistrations
                .Include(r => r.Activity)
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            if (registration == null) return false;

            registration.Status = newStatus;
            registration.AdminRemarks = adminRemarks;
            registration.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (registration.UserId.HasValue)
            {
                var activityTitle = registration.Activity?.Title ?? "Islamic Program";
                if (newStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.CreateNotificationAsync(
                        registration.UserId.Value,
                        "✅ Registration Approved!",
                        $"Congratulations! Your participation in '{activityTitle}' is confirmed. Venue: {registration.Activity?.Location ?? "TBA"}.",
                        "activities",
                        "/user-profile"
                    );
                }
                else if (newStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.CreateNotificationAsync(
                        registration.UserId.Value,
                        "❌ Registration Status Update",
                        $"Your registration request for '{activityTitle}' could not be accepted at this time. Remarks: {adminRemarks ?? "Capacity reached"}.",
                        "activities",
                        "/user-profile"
                    );
                }
            }

            return true;
        }
    }
}
