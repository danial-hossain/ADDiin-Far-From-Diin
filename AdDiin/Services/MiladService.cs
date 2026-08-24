using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IMiladService
    {
        Task<MiladRequest> CreateAsync(MiladCreateViewModel model, int userId);
        Task<List<MiladRequest>> GetUserRequestsAsync(int userId);
        Task<MiladRequest?> GetByIdAsync(int id);
        Task<MiladRequest?> UpdateAsync(int id, MiladCreateViewModel model, int userId);
        Task<bool> CancelAsync(int id, int userId);
        Task<List<MiladRequest>> GetAdminListAsync(string? status = null, string? search = null);
        Task<MiladRequest?> UpdateStatusAsync(int id, string status, string? adminRemark);
    }

    public class MiladService : IMiladService
    {
        private readonly ApplicationDbContext _context;

        public MiladService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MiladRequest> CreateAsync(MiladCreateViewModel model, int userId)
        {
            var milad = new MiladRequest
            {
                UserId = userId,
                Name = model.Name,
                Phone = model.Phone,
                Description = model.Description,
                MiladDate = model.MiladDate,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.MiladRequests.Add(milad);
            await _context.SaveChangesAsync();
            return milad;
        }

        public async Task<List<MiladRequest>> GetUserRequestsAsync(int userId)
        {
            return await _context.MiladRequests
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<MiladRequest?> GetByIdAsync(int id)
        {
            return await _context.MiladRequests
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MiladRequest?> UpdateAsync(int id, MiladCreateViewModel model, int userId)
        {
            var milad = await _context.MiladRequests.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (milad == null || milad.Status != "pending") return null;

            milad.Name = model.Name;
            milad.Phone = model.Phone;
            milad.Description = model.Description;
            milad.MiladDate = model.MiladDate;
            milad.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return milad;
        }

        public async Task<bool> CancelAsync(int id, int userId)
        {
            var milad = await _context.MiladRequests.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (milad == null || milad.Status != "pending") return false;

            _context.MiladRequests.Remove(milad);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MiladRequest>> GetAdminListAsync(string? status = null, string? search = null)
        {
            var query = _context.MiladRequests.Include(m => m.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
                query = query.Where(m => m.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(m => m.Name.Contains(s) || m.Phone.Contains(s) || (m.User != null && m.User.FullName.Contains(s)));
            }

            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }

        public async Task<MiladRequest?> UpdateStatusAsync(int id, string status, string? adminRemark)
        {
            var milad = await _context.MiladRequests.Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
            if (milad == null) return null;

            milad.Status = status.ToLower();
            if (!string.IsNullOrWhiteSpace(adminRemark))
            {
                milad.AdminRemark = adminRemark;
            }
            milad.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return milad;
        }
    }
}
