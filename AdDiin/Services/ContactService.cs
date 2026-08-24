using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IContactService
    {
        Task<ContactMessage> SubmitMessageAsync(ContactViewModel model);
        Task<List<ContactMessage>> GetAllMessagesAsync();
        Task<ContactMessage?> GetByIdAsync(int id);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> ReplyMessageAsync(int id, string replyText);
        Task<bool> DeleteMessageAsync(int id);
    }

    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContactMessage> SubmitMessageAsync(ContactViewModel model)
        {
            var msg = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Company = model.Company,
                Message = model.Message,
                Status = "unread",
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactMessages.Add(msg);
            await _context.SaveChangesAsync();
            return msg;
        }

        public async Task<List<ContactMessage>> GetAllMessagesAsync()
        {
            return await _context.ContactMessages
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<ContactMessage?> GetByIdAsync(int id)
        {
            return await _context.ContactMessages.FindAsync(id);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return false;

            msg.Status = "read";
            msg.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReplyMessageAsync(int id, string replyText)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return false;

            msg.ReplyMessage = replyText;
            msg.Status = "replied";
            msg.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return false;

            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
