using AdDiin.Models.Entities;
using AdDiin.Services;
using AdDiin.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AdDiin.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessagingService _messagingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<SupportChatHub> _hubContext;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            IMessagingService messagingService,
            UserManager<ApplicationUser> userManager,
            IHubContext<SupportChatHub> hubContext,
            ILogger<MessagesController> logger)
        {
            _messagingService = messagingService;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = User.IsInRole("Admin");
            var conversations = await _messagingService.GetUserConversationsAsync(user.Id, isAdmin);

            Conversation? activeConversation = null;
            if (id.HasValue)
            {
                activeConversation = await _messagingService.GetConversationWithMessagesAsync(id.Value, user.Id, isAdmin);
            }
            else if (conversations.Any())
            {
                activeConversation = await _messagingService.GetConversationWithMessagesAsync(conversations.First().Id, user.Id, isAdmin);
            }
            else if (!isAdmin)
            {
                // Auto create initial conversation for regular user
                activeConversation = await _messagingService.GetOrCreateConversationAsync(user.Id);
                conversations = await _messagingService.GetUserConversationsAsync(user.Id, isAdmin);
            }

            ViewBag.ActiveConversation = activeConversation;
            return isAdmin
                ? View("~/Views/Admin/Messages.cshtml", conversations)
                : View(conversations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string messageContent)
        {
            var isAdmin = User.IsInRole("Admin");
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return isAdmin
                    ? RedirectToAction("Messages", "Admin", new { id = conversationId })
                    : RedirectToAction(nameof(Index), new { id = conversationId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var msg = await _messagingService.SendMessageAsync(conversationId, user.Id, messageContent.Trim(), isAdmin);

            if (msg == null)
            {
                TempData["ErrorMessage"] = "Failed to send message. Please verify conversation status.";
            }
            else
            {
                try
                {
                    await BroadcastMessageAsync(msg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message {MessageId} was saved but live notification failed.", msg.Id);
                }
            }

            return isAdmin
                ? RedirectToAction("Messages", "Admin", new { id = conversationId })
                : RedirectToAction(nameof(Index), new { id = conversationId });
        }

        [HttpGet("/api/contact/conversations")]
        public async Task<IActionResult> ConversationsApi()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var conversations = await _messagingService.GetUserConversationsAsync(user.Id, isAdmin);

            if (!isAdmin && conversations.Count == 0)
            {
                await _messagingService.GetOrCreateConversationAsync(user.Id);
                conversations = await _messagingService.GetUserConversationsAsync(user.Id, false);
            }

            return Ok(conversations.Select(conversation => ToConversationResponse(conversation, isAdmin)));
        }

        [HttpGet("/api/contact/conversations/{id:int}/messages")]
        public async Task<IActionResult> ConversationMessagesApi(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var conversation = await _messagingService.GetConversationWithMessagesAsync(
                id, user.Id, User.IsInRole("Admin"));
            if (conversation == null) return NotFound();

            return Ok(new
            {
                conversation = ToConversationResponse(conversation, User.IsInRole("Admin")),
                messages = conversation.Messages.OrderBy(message => message.CreatedAt).Select(message => ToMessageResponse(message))
            });
        }

        [HttpPost("/api/contact/conversations/{id:int}/messages")]
        public async Task<IActionResult> SendMessageApi(int id, [FromBody] SendMessageRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { error = "Message cannot be empty." });
            }
            if (request.Content.Trim().Length > MessagingService.MaxMessageLength)
            {
                return BadRequest(new { error = $"Message cannot exceed {MessagingService.MaxMessageLength} characters." });
            }

            var isAdmin = User.IsInRole("Admin");
            var authorizedConversation = await _messagingService.GetConversationWithMessagesAsync(
                id, user.Id, isAdmin);
            if (authorizedConversation == null) return NotFound();

            var message = await _messagingService.SendMessageAsync(
                id, user.Id, request.Content, isAdmin);
            if (message == null)
            {
                return Conflict(new { error = "This conversation is closed or unavailable." });
            }

            try
            {
                await BroadcastMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message {MessageId} was saved but live notification failed.", message.Id);
            }

            return Ok(ToMessageResponse(message));
        }

        [HttpPost("/api/contact/conversations/{id:int}/read")]
        public async Task<IActionResult> MarkReadApi(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var marked = await _messagingService.MarkConversationReadAsync(
                id, user.Id, User.IsInRole("Admin"));
            if (marked)
            {
                var conversations = await _messagingService.GetUserConversationsAsync(
                    user.Id, User.IsInRole("Admin"));
                var conversation = conversations.FirstOrDefault(item => item.Id == id);
                if (conversation != null)
                {
                    try
                    {
                        await BroadcastReadReceiptAsync(conversation, User.IsInRole("Admin"));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Read receipt notification failed for conversation {ConversationId}.", id);
                    }
                }
            }

            return marked ? Ok(new { success = true }) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CloseConversation(int id)
        {
            await _messagingService.CloseConversationAsync(id);
            TempData["SuccessMessage"] = "Conversation closed.";
            return RedirectToAction(nameof(Index), new { id });
        }

        private async Task BroadcastMessageAsync(Message message)
        {
            if (message.Conversation == null)
            {
                return;
            }

            var payload = ToMessageResponse(message, message.Conversation.UserId);
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = adminUsers
                .Select(admin => admin.Id.ToString())
                .ToArray();

            if (message.SenderType == "user")
            {
                if (adminIds.Length > 0)
                {
                    await _hubContext.Clients.Users(adminIds)
                        .SendAsync("ReceiveSupportMessage", payload);
                }
            }

            else
            {
                await _hubContext.Clients.User(message.Conversation.UserId.ToString())
                    .SendAsync("ReceiveSupportMessage", payload);
            }
        }

        private async Task BroadcastReadReceiptAsync(Conversation conversation, bool readerIsAdmin)
        {
            var payload = new
            {
                conversationId = conversation.Id,
                readerType = readerIsAdmin ? "admin" : "user"
            };

            if (readerIsAdmin)
            {
                await _hubContext.Clients.User(conversation.UserId.ToString())
                    .SendAsync("ConversationMessagesRead", payload);
                return;
            }

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = adminUsers.Select(admin => admin.Id.ToString()).ToArray();
            if (adminIds.Length > 0)
            {
                await _hubContext.Clients.Users(adminIds)
                    .SendAsync("ConversationMessagesRead", payload);
            }
        }

        private static object ToConversationResponse(Conversation conversation, bool isAdmin)
        {
            var lastMessage = conversation.Messages
                .OrderByDescending(message => message.CreatedAt)
                .FirstOrDefault();

            return new
            {
                id = conversation.Id,
                subject = conversation.Subject,
                status = conversation.Status,
                userId = conversation.UserId,
                userDisplayName = conversation.User?.FullName ?? "User",
                userEmail = conversation.User?.Email,
                updatedAt = conversation.UpdatedAt ?? conversation.CreatedAt,
                unreadCount = conversation.Messages.Count(message =>
                    !message.IsRead && message.SenderType == (isAdmin ? "user" : "admin")),
                lastMessage = lastMessage == null ? null : ToMessageResponse(lastMessage)
            };
        }

        private static object ToMessageResponse(Message message, int? conversationUserId = null)
        {
            return new
            {
                id = message.Id,
                conversationId = message.ConversationId,
                senderType = message.SenderType,
                senderUserId = message.SenderId,
                senderDisplayName = message.Sender?.FullName
                    ?? (message.SenderType == "admin" ? "Support" : "User"),
                content = message.MessageContent,
                createdAt = message.CreatedAt,
                isRead = message.IsRead,
                recipientUserId = conversationUserId
            };
        }

        public sealed class SendMessageRequest
        {
            public string Content { get; set; } = string.Empty;
        }
    }
}
