using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IPrayerTimeService
    {
        Task<List<PrayerTime>> GetAllAsync();
        Task<List<PrayerTime>> GetJamaatTimesAsync();
        Task<List<PrayerTime>> GetAzanTimesAsync();
        Task<List<PrayerTime>> GetNaflTimesAsync();
        Task<PrayerTime?> GetByIdAsync(int id);
        Task<PrayerTime> CreateAsync(PrayerTime prayer);
        Task<PrayerTime?> UpdateAsync(int id, PrayerTime updated);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<(PrayerTime? NextPrayer, TimeSpan TimeRemaining)> GetNextPrayerAsync();
    }

    public class PrayerTimeService : IPrayerTimeService
    {
        private readonly ApplicationDbContext _context;

        public PrayerTimeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PrayerTime>> GetAllAsync()
        {
            return await _context.PrayerTimes
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<PrayerTime>> GetJamaatTimesAsync()
        {
            return await _context.PrayerTimes
                .Where(p => p.IsActive && p.PrayerType == "jamaat")
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<PrayerTime>> GetAzanTimesAsync()
        {
            return await _context.PrayerTimes
                .Where(p => p.IsActive && p.PrayerType == "azan")
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<PrayerTime>> GetNaflTimesAsync()
        {
            return await _context.PrayerTimes
                .Where(p => p.IsActive && p.Category == "nafl")
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PrayerTime?> GetByIdAsync(int id)
        {
            return await _context.PrayerTimes.FindAsync(id);
        }

        public async Task<PrayerTime> CreateAsync(PrayerTime prayer)
        {
            prayer.CreatedAt = DateTime.UtcNow;
            _context.PrayerTimes.Add(prayer);
            await _context.SaveChangesAsync();
            return prayer;
        }

        public async Task<PrayerTime?> UpdateAsync(int id, PrayerTime updated)
        {
            var prayer = await _context.PrayerTimes.FindAsync(id);
            if (prayer == null) return null;

            prayer.PrayerName = updated.PrayerName;
            prayer.PrayerTimeValue = updated.PrayerTimeValue;
            prayer.DisplayNameEn = updated.DisplayNameEn;
            prayer.DisplayNameBn = updated.DisplayNameBn;
            prayer.Category = updated.Category;
            prayer.PrayerType = updated.PrayerType;
            prayer.DisplayOrder = updated.DisplayOrder;
            prayer.IsActive = updated.IsActive;
            prayer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return prayer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var prayer = await _context.PrayerTimes.FindAsync(id);
            if (prayer == null) return false;

            _context.PrayerTimes.Remove(prayer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var prayer = await _context.PrayerTimes.FindAsync(id);
            if (prayer == null) return false;

            prayer.IsActive = !prayer.IsActive;
            prayer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return prayer.IsActive;
        }

        public async Task<(PrayerTime? NextPrayer, TimeSpan TimeRemaining)> GetNextPrayerAsync()
        {
            var nowTime = DateTime.Now.TimeOfDay;
            var azanPrayers = await _context.PrayerTimes
                .Where(p => p.IsActive && (p.PrayerType == "azan" || p.PrayerType == "fard" || p.Category == "fard"))
                .OrderBy(p => p.PrayerTimeValue)
                .ToListAsync();

            if (!azanPrayers.Any()) return (null, TimeSpan.Zero);

            var next = azanPrayers.FirstOrDefault(p => p.PrayerTimeValue > nowTime);
            if (next != null)
            {
                return (next, next.PrayerTimeValue - nowTime);
            }

            // Next prayer is Fajr tomorrow
            var fajr = azanPrayers.First();
            var timeRemaining = (TimeSpan.FromHours(24) - nowTime) + fajr.PrayerTimeValue;
            return (fajr, timeRemaining);
        }
    }
}
