using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IMessagingService
    {
        Task<List<Conversation>> GetUserConversationsAsync(int userId, bool isAdmin);
        Task<Conversation> GetOrCreateConversationAsync(int userId, string subject = "Support Request");
        Task<Conversation?> GetConversationWithMessagesAsync(int conversationId, int currentUserId, bool isAdmin);
        Task<Message?> SendMessageAsync(int conversationId, int senderId, string content, bool isAdmin);
        Task<bool> CloseConversationAsync(int conversationId);
        Task<int> GetUnreadCountAsync(int userId, bool isAdmin);
    }

    public class MessagingService : IMessagingService
    {
        private readonly ApplicationDbContext _context;

        public MessagingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return await _context.Conversations
                    .Include(c => c.User)
                    .Include(c => c.Admin)
                    .Include(c => c.Messages)
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .ToListAsync();
            }

            return await _context.Conversations
                .Where(c => c.UserId == userId)
                .Include(c => c.Admin)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Conversation> GetOrCreateConversationAsync(int userId, string subject = "Support Request")
        {
            var conversation = await _context.Conversations
                .Include(c => c.User)
                .Include(c => c.Admin)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "active");

            if (conversation != null) return conversation;

            conversation = new Conversation
            {
                UserId = userId,
                Subject = subject,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            await _context.Entry(conversation).Reference(c => c.User).LoadAsync();
            return conversation;
        }

        public async Task<Conversation?> GetConversationWithMessagesAsync(int conversationId, int currentUserId, bool isAdmin)
        {
            var conversation = await _context.Conversations
                .Include(c => c.User)
                .Include(c => c.Admin)
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return null;

            if (!isAdmin && conversation.UserId != currentUserId)
            {
                return null; // unauthorized
            }

            // Mark unread messages sent by the other party as read
            var unreadMessages = conversation.Messages
                .Where(m => m.SenderId != currentUserId && !m.IsRead)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                    msg.ReadAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            return conversation;
        }

        public async Task<Message?> SendMessageAsync(int conversationId, int senderId, string content, bool isAdmin)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return null;

            if (!isAdmin && conversation.UserId != senderId)
            {
                return null;
            }

            if (isAdmin && conversation.AdminId == null)
            {
                conversation.AdminId = senderId;
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                MessageContent = content,
                SenderType = isAdmin ? "admin" : "user",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            conversation.UpdatedAt = DateTime.UtcNow;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();
            return message;
        }

        public async Task<bool> CloseConversationAsync(int conversationId)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return false;

            conversation.Status = "closed";
            conversation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return await _context.Messages
                    .Where(m => m.SenderType == "user" && !m.IsRead)
                    .CountAsync();
            }

            return await _context.Messages
                .Where(m => m.Conversation != null && m.Conversation.UserId == userId && m.SenderType == "admin" && !m.IsRead)
                .CountAsync();
        }
    }
}
