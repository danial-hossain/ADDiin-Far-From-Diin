using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IIslamicEventService
    {
        Task<List<IslamicEvent>> GetUpcomingEventsAsync();
        Task<List<IslamicEvent>> GetAllEventsAsync();
        Task<IslamicEvent?> GetByIdAsync(int id);
        Task<IslamicEvent> CreateAsync(IslamicEvent evt);
        Task<IslamicEvent?> UpdateAsync(int id, IslamicEvent updated);
        Task<bool> DeleteAsync(int id);
    }

    public class IslamicEventService : IIslamicEventService
    {
        private readonly ApplicationDbContext _context;

        public IslamicEventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<IslamicEvent>> GetUpcomingEventsAsync()
        {
            return await _context.IslamicEvents
                .Where(e => e.IsActive && e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<List<IslamicEvent>> GetAllEventsAsync()
        {
            return await _context.IslamicEvents
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<IslamicEvent?> GetByIdAsync(int id)
        {
            return await _context.IslamicEvents.FindAsync(id);
        }

        public async Task<IslamicEvent> CreateAsync(IslamicEvent evt)
        {
            evt.CreatedAt = DateTime.UtcNow;
            _context.IslamicEvents.Add(evt);
            await _context.SaveChangesAsync();
            return evt;
        }

        public async Task<IslamicEvent?> UpdateAsync(int id, IslamicEvent updated)
        {
            var evt = await _context.IslamicEvents.FindAsync(id);
            if (evt == null) return null;

            evt.EventName = updated.EventName;
            evt.EventDate = updated.EventDate;
            evt.HijriDate = updated.HijriDate;
            evt.HijriMonth = updated.HijriMonth;
            evt.HijriDay = updated.HijriDay;
            evt.EventType = updated.EventType;
            evt.Description = updated.Description;
            evt.IsActive = updated.IsActive;
            evt.DisplayOrder = updated.DisplayOrder;
            evt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return evt;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var evt = await _context.IslamicEvents.FindAsync(id);
            if (evt == null) return false;

            _context.IslamicEvents.Remove(evt);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
