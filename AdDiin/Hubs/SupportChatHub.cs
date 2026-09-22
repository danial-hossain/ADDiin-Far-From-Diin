using AdDiin.Models.Entities;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace AdDiin.Hubs;

[Authorize]
/// <summary>
/// Exposes authenticated support-chat presence without allowing client-selected
/// SignalR groups to bypass conversation authorization.
/// </summary>
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

    /// <summary>
    /// Confirms that the caller may access the requested conversation.
    /// </summary>
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
