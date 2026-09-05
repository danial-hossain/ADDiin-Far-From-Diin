using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class DiinAIController : Controller
    {
        private readonly IDiinAIService _aiService;

        public DiinAIController(IDiinAIService aiService)
        {
            _aiService = aiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new { success = false, message = "Question cannot be empty" });
            }

            var (answer, sources) = await _aiService.AskIslamicQuestionAsync(request.Message, request.History);

            return Json(new
            {
                success = true,
                response = answer,
                sources = sources.Select(s => new { source = s.Source, reference = s.Reference }),
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }

        [HttpPost]
        [Route("api/ai/ask")]
        public async Task<IActionResult> Ask([FromBody] AIApiAskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return BadRequest(new { error = "Query cannot be empty" });
            }

            var (answer, sources) = await _aiService.AskIslamicQuestionAsync(request.Query);

            return Ok(new AIApiAskResponse
            {
                Answer = answer,
                Sources = sources
            });
        }

        [HttpGet]
        [Route("api/ai/health")]
        public async Task<IActionResult> Health()
        {
            var (isHealthy, details) = await _aiService.CheckHealthAsync();
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
