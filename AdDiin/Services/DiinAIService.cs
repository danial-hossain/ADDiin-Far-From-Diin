using AdDiin.Models.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdDiin.Services
{
    public interface IDiinAIService
    {
        Task<(string Answer, List<DiinAISource> Sources)> AskIslamicQuestionAsync(string question, List<DiinAIChatMessage>? history = null);
        bool IsOffTopic(string query);
    }

    public class DiinAIService : IDiinAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiinAIService> _logger;

        private static readonly string[] OffTopicKeywords =
        {
            "movie", "film", "নাটক", "সিনেমা", "গান", "music", "cricket", "football", "খেলা", "game",
            "politics", "রাজনীতি", "love", "প্রেম", "girlfriend", "boyfriend", "sex", "cooking",
            "recipe", "রান্না", "stock", "share market", "crypto", "hack", "হ্যাক", "joke", "funny",
            "entertainment", "gossip", "weather", "আবহাওয়া", "tiktok", "youtube", "instagram",
            "facebook", "python", "javascript", "react", "programming", "coding", "visa", "passport"
        };

        public DiinAIService(HttpClient httpClient, IConfiguration configuration, ILogger<DiinAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public bool IsOffTopic(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            var lower = query.ToLower();
            return OffTopicKeywords.Any(k => lower.Contains(k.ToLower()));
        }

        public async Task<(string Answer, List<DiinAISource> Sources)> AskIslamicQuestionAsync(string question, List<DiinAIChatMessage>? history = null)
        {
            // 1. Guard against off-topic queries
            if (IsOffTopic(question))
            {
                return (
                    "আমি শুধুমাত্র ইসলামিক বিষয়ে সাহায্য করতে পারি। যেমন:\n\n" +
                    "• কুরআন ও তাফসির\n" +
                    "• হাদিস ও সুন্নাহ\n" +
                    "• নামাজ, রোজা, যাকাত ও হজ\n" +
                    "• আকিদা ও ফিকহ\n" +
                    "• ইসলামিক ইতিহাস ও নবীগণের জীবনী\n" +
                    "• হালাল-হারাম বিষয়াদি\n\n" +
                    "অনুগ্রহ করে ইসলাম সম্পর্কিত প্রশ্ন করুন। 🕌",
                    new List<DiinAISource>()
                );
            }

            // 2. Resolve AI Backend URL (checks AI_BACKEND_URL env var, then AISettings:BackendUrl in appsettings)
            var backendUrl = _configuration["AI_BACKEND_URL"] 
                             ?? _configuration["AISettings:BackendUrl"];

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                _logger.LogWarning("AI Backend URL is not configured. Set AI_BACKEND_URL environment variable or AISettings:BackendUrl in appsettings.json.");
                return (
                    "দুঃখিত, AI সার্ভার কনফিগারেশন এখনো সম্পন্ন হয়নি। অনুগ্রহ করে পরবর্তীতে আবার চেষ্টা করুন।",
                    new List<DiinAISource>()
                );
            }

            // 3. Call Colab FastAPI Backend
            try
            {
                var endpoint = $"{backendUrl.TrimEnd('/')}/ask";
                var payload = new { query = question };

                _logger.LogInformation("Sending question to AI backend. URL: {BackendUrl}", endpoint);

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                using var response = await _httpClient.PostAsJsonAsync(endpoint, payload);

                _logger.LogInformation("AI backend response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AIApiAskResponse>(jsonOptions);
                    if (result != null && !string.IsNullOrWhiteSpace(result.Answer))
                    {
                        return (result.Answer, result.Sources ?? new List<DiinAISource>());
                    }
                    _logger.LogWarning("AI backend returned empty or unparseable answer.");
                }
                else
                {
                    _logger.LogError("AI Backend returned non-success status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout occurred while connecting to Colab AI Backend at {BackendUrl}", backendUrl);
                return (
                    "দুঃখিত, AI সার্ভারের সাথে বর্তমানে সংযোগ করা যাচ্ছে না",
                    new List<DiinAISource>()
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Colab AI Backend at {BackendUrl}", backendUrl);
                return (
                    "দুঃখিত, AI সার্ভারের সাথে বর্তমানে সংযোগ করা যাচ্ছে না",
                    new List<DiinAISource>()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DiinAIService while calling AI Backend at {BackendUrl}", backendUrl);
            }

            return (
                "দুঃখিত, AI সার্ভারের সাথে বর্তমানে সংযোগ করা যাচ্ছে না",
                new List<DiinAISource>()
            );
        }
    }
}
