using AdDiin.Models.Entities;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace AdDiin.Hubs;

[Authorize]
public class SupportChatHub : Hub
{
    private readonly IMessagingService _messagingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SupportChatHub(
        IMessagingService messagingService,
        UserManager<ApplicationUser> userManager)
    {
        _messagingService = messagingService;
        _userManager = userManager;
    }

    // No client-controlled groups are used. The server addresses recipients by
    // their authenticated user IDs when a message is saved by the controller.
    public async Task<bool> SubscribeToConversation(int conversationId)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return false;
        }

        var isAdmin = Context.User?.IsInRole("Admin") == true;
        var conversation = await _messagingService.GetConversationWithMessagesAsync(
            conversationId, userId.Value, isAdmin);
        return conversation != null;
    }

    private int? GetUserId()
    {
        return int.TryParse(Context.UserIdentifier, out var userId) ? userId : null;
    }
}
