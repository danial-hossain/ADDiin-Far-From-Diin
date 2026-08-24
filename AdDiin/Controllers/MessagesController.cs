using AdDiin.Models.Entities;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessagingService _messagingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(IMessagingService messagingService, UserManager<ApplicationUser> userManager)
        {
            _messagingService = messagingService;
            _userManager = userManager;
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
            return View(conversations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string messageContent)
        {
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return RedirectToAction(nameof(Index), new { id = conversationId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = User.IsInRole("Admin");
            var msg = await _messagingService.SendMessageAsync(conversationId, user.Id, messageContent.Trim(), isAdmin);

            if (msg == null)
            {
                TempData["ErrorMessage"] = "Failed to send message. Please verify conversation status.";
            }

            return RedirectToAction(nameof(Index), new { id = conversationId });
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
    }
}
