using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IDonationService
    {
        Task<Donation> InitiateDonationAsync(DonationInitiateViewModel model, int? userId = null);
        Task<Donation?> ProcessPaymentSuccessAsync(string tranId, string? valId = null, string? bankTranId = null, string? method = null);
        Task<Donation?> ProcessPaymentFailAsync(string tranId);
        Task<Donation?> ProcessPaymentCancelAsync(string tranId);
        Task<Donation?> GetByTranIdAsync(string tranId);
        Task<List<Donation>> GetUserDonationsAsync(int userId);
        Task<AdminDonationsViewModel> GetAdminDonationsAsync(string? category = null, string? status = null, string? search = null);
        Task<Dictionary<string, decimal>> GetCategoryBreakdownAsync();
    }

    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _context;

        public DonationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Donation> InitiateDonationAsync(DonationInitiateViewModel model, int? userId = null)
        {
            var tranId = $"DON_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var donation = new Donation
            {
                UserId = userId,
                Name = model.IsAnonymous ? null : model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Category = model.Category.ToLower(),
                Amount = model.Amount,
                Currency = "BDT",
                TranId = tranId,
                PaymentStatus = "pending",
                PaymentMethod = model.PaymentMethod ?? "sslcommerz",
                IsAnonymous = model.IsAnonymous,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<Donation?> ProcessPaymentSuccessAsync(string tranId, string? valId = null, string? bankTranId = null, string? method = null)
        {
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.TranId == tranId);
            if (donation == null) return null;

            donation.PaymentStatus = "completed";
            donation.ValId = valId ?? $"VAL_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            donation.BankTranId = bankTranId ?? $"BANK_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            if (!string.IsNullOrEmpty(method)) donation.PaymentMethod = method;
            donation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<Donation?> ProcessPaymentFailAsync(string tranId)
        {
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.TranId == tranId);
            if (donation == null) return null;

            donation.PaymentStatus = "failed";
            donation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<Donation?> ProcessPaymentCancelAsync(string tranId)
        {
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.TranId == tranId);
            if (donation == null) return null;

            donation.PaymentStatus = "cancelled";
            donation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<Donation?> GetByTranIdAsync(string tranId)
        {
            return await _context.Donations
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.TranId == tranId);
        }

        public async Task<List<Donation>> GetUserDonationsAsync(int userId)
        {
            return await _context.Donations
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<AdminDonationsViewModel> GetAdminDonationsAsync(string? category = null, string? status = null, string? search = null)
        {
            var query = _context.Donations.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(d => d.Category == category);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(d => d.PaymentStatus == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(d =>
                    (d.Name != null && d.Name.Contains(s)) ||
                    (d.Email != null && d.Email.Contains(s)) ||
                    (d.Phone != null && d.Phone.Contains(s)) ||
                    d.TranId.Contains(s));
            }

            var list = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

            var totalCompleted = await _context.Donations
                .Where(d => d.PaymentStatus == "completed")
                .SumAsync(d => d.Amount);

            var totalCount = await _context.Donations.CountAsync();
            var pendingCount = await _context.Donations.CountAsync(d => d.PaymentStatus == "pending");

            var breakdown = await GetCategoryBreakdownAsync();

            return new AdminDonationsViewModel
            {
                Donations = list,
                TotalCompletedAmount = totalCompleted,
                TotalCount = totalCount,
                PendingCount = pendingCount,
                SelectedCategory = category,
                SelectedStatus = status,
                SearchQuery = search,
                CategoryBreakdown = breakdown
            };
        }

        public async Task<Dictionary<string, decimal>> GetCategoryBreakdownAsync()
        {
            return await _context.Donations
                .Where(d => d.PaymentStatus == "completed")
                .GroupBy(d => d.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(d => d.Amount) })
                .ToDictionaryAsync(k => k.Category, v => v.Total);
        }
    }
}
