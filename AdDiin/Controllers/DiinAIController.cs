using AdDiin.Models.ViewModels;
using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Services;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    /// <summary>
    /// Handles Diin AI requests, optional account-backed conversation history,
    /// and the health endpoint used by the client.
    /// </summary>
    public class DiinAIController : Controller
    {
        private readonly IDiinAIService _aiService;
        private readonly ApplicationDbContext _context;

        public DiinAIController(IDiinAIService aiService, ApplicationDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        /// <summary>
        /// Serves the lightweight chat endpoint used by the initial chat client.
        /// </summary>
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new
                {
                    success = false,
                    message = "Question cannot be empty"
                });
            }

            var (answer, sources) =
                await _aiService.AskIslamicQuestionAsync(
                    request.Message,
                    request.History);

            return Json(new
            {
                success = true,
                response = answer,
                sources = sources.Select(s => new
                {
                    source = s.Source,
                    reference = s.Reference
                }),
                contactFallback = DiinAIService.IsContactFallback(answer),
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }

        [HttpPost]
        [Route("api/ai/ask")]
        /// <summary>
        /// Loads the user's active conversation, sends the last 20 messages to
        /// the AI service, and persists both sides of the exchange.
        /// </summary>
        public async Task<IActionResult> Ask([FromBody] AIApiAskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return BadRequest(new
                {
                    error = "Query cannot be empty"
                });
            }

            var userId = GetCurrentUserId();
            DiinAIConversation? conversation = null;
            List<DiinAIChatMessage>? history = null;

            // Guests can ask questions without persistence; authenticated users
            // receive conversation history and a saved assistant response.
            if (userId.HasValue)
            {
                conversation = request.ConversationId.HasValue
                    ? await GetConversationAsync(
                        userId.Value,
                        request.ConversationId.Value)
                    : null;

                conversation ??=
                    await GetOrCreateActiveConversationAsync(userId.Value);

                await ActivateConversationAsync(
                    userId.Value,
                    conversation);

                history = conversation.Messages
                    .OrderBy(message => message.CreatedAt)
                    .TakeLast(20)
                    .Select(message => new DiinAIChatMessage
                    {
                        Role = message.Role,
                        Content = message.Content,
                        Timestamp = message.CreatedAt
                    })
                    .ToList();

                conversation.Messages.Add(new DiinAIMessage
                {
                    Role = "user",
                    Content = request.Query.Trim()
                });
            }

            var (answer, sources) =
                await _aiService.AskIslamicQuestionAsync(
                    request.Query,
                    history);

            if (conversation != null)
            {
                // Store the serialized citations with the assistant message so
                // history can render the same source cards later.
                conversation.Messages.Add(new DiinAIMessage
                {
                    Role = "assistant",
                    Content = answer,
                    SourcesJson = JsonSerializer.Serialize(sources)
                });

                conversation.Title = conversation.Title == "New Diin AI chat"
                    ? request.Query.Trim()[
                        ..Math.Min(request.Query.Trim().Length, 255)]
                    : conversation.Title;

                conversation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            var apiResponse = new AIApiAskResponse
            {
                Answer = answer,
                Sources = sources.Select(source => new DiinAISource
                {
                    Id = source.Id,
                    Source = source.Source,
                    Reference = source.Reference,
                    Text = source.Text
                }).ToList()
            };

            apiResponse.ConversationId = conversation?.Id;
            apiResponse.ContactFallback =
                DiinAIService.IsContactFallback(answer);

            return Ok(apiResponse);
        }

        [HttpGet]
        [Route("api/ai/history")]
        /// <summary>
        /// Returns the selected authenticated conversation or the active one.
        /// </summary>
        public async Task<IActionResult> History(
            [FromQuery] int? conversationId = null)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Ok(new
                {
                    authenticated = false,
                    conversationId = (int?)null,
                    messages = Array.Empty<object>()
                });
            }

            var conversation = await _context.DiinAIConversations
                .Include(item => item.Messages)
                .Where(item =>
                    item.UserId == userId.Value &&
                    (!conversationId.HasValue
                        ? item.IsActive
                        : item.Id == conversationId.Value))
                .OrderByDescending(item => item.UpdatedAt)
                .FirstOrDefaultAsync();

            var messages = conversation?.Messages
                .OrderBy(message => message.CreatedAt)
                .Select(message => new
                {
                    role = message.Role,
                    content = message.Content,
                    sources = string.IsNullOrWhiteSpace(message.SourcesJson)
                        ? Array.Empty<object>()
                        : JsonSerializer.Deserialize<object[]>(
                            message.SourcesJson) ?? Array.Empty<object>(),
                    timestamp = message.CreatedAt
                })
                .Cast<object>()
                .ToArray() ?? Array.Empty<object>();

            return Ok(new
            {
                authenticated = true,
                conversationId = conversation?.Id,
                messages
            });
        }

        [HttpGet]
        [Route("api/ai/conversations")]
        /// <summary>
        /// Returns conversation summaries for the signed-in user's history list.
        /// </summary>
        public async Task<IActionResult> Conversations()
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Ok(new
                {
                    authenticated = false,
                    conversations = Array.Empty<object>()
                });
            }

            var conversations = await _context.DiinAIConversations
                .Where(item => item.UserId == userId.Value)
                .OrderByDescending(item => item.UpdatedAt)
                .Select(item => new
                {
                    id = item.Id,
                    title = item.Title,
                    updatedAt = item.UpdatedAt,
                    messageCount = item.Messages.Count
                })
                .ToListAsync();

            return Ok(new
            {
                authenticated = true,
                conversations
            });
        }

        [HttpPost]
        [Route("api/ai/new-chat")]
        /// <summary>
        /// Closes active conversations so the next question starts a new thread.
        /// </summary>
        public async Task<IActionResult> NewChat()
        {
            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                var activeConversations =
                    await _context.DiinAIConversations
                        .Where(item =>
                            item.UserId == userId.Value &&
                            item.IsActive)
                        .ToListAsync();

                foreach (var conversation in activeConversations)
                {
                    conversation.IsActive = false;
                    conversation.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true
            });
        }

        private int? GetCurrentUserId()
        {
            // Identity stores the user key in the standard NameIdentifier claim.
            var value = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(value, out var userId)
                ? userId
                : null;
        }

        private async Task<DiinAIConversation>
            GetOrCreateActiveConversationAsync(int userId)
        {
            // Only one active conversation is selected for the chat composer.
            var conversation = await _context.DiinAIConversations
                .Include(item => item.Messages)
                .Where(item =>
                    item.UserId == userId &&
                    item.IsActive)
                .OrderByDescending(item => item.UpdatedAt)
                .FirstOrDefaultAsync();

            if (conversation != null)
            {
                return conversation;
            }

            conversation = new DiinAIConversation
            {
                UserId = userId
            };

            _context.DiinAIConversations.Add(conversation);

            await _context.SaveChangesAsync();

            return conversation;
        }

        private Task<DiinAIConversation?> GetConversationAsync(
            int userId,
            int conversationId)
        {
            return _context.DiinAIConversations
                .Include(item => item.Messages)
                .FirstOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.Id == conversationId);
        }

        private async Task ActivateConversationAsync(
            int userId,
            DiinAIConversation selectedConversation)
        {
            // Selecting a saved thread deactivates the other threads for this user.
            var activeConversations =
                await _context.DiinAIConversations
                    .Where(item =>
                        item.UserId == userId &&
                        item.IsActive &&
                        item.Id != selectedConversation.Id)
                    .ToListAsync();

            foreach (var conversation in activeConversations)
            {
                conversation.IsActive = false;
            }

            selectedConversation.IsActive = true;
        }

        [HttpGet]
        [Route("api/ai/health")]
        /// <summary>
        /// Reports whether the configured AI backend is reachable.
        /// </summary>
        public async Task<IActionResult> Health()
        {
            var (isHealthy, details) =
                await _aiService.CheckHealthAsync();

            return Ok(new
            {
                connected = isHealthy,
                details = details,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
            public List<DiinAIChatMessage>? History { get; set; }
        }
    }
}