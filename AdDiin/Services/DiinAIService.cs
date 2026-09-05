using AdDiin.Models.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdDiin.Services
{
    public interface IDiinAIService
    {
        Task<(string Answer, List<DiinAISource> Sources)> AskIslamicQuestionAsync(
            string question,
            List<DiinAIChatMessage>? history = null
        );

        Task<(bool IsHealthy, string Details)> CheckHealthAsync();

        bool IsOffTopic(string query);
    }

    public class DiinAIService : IDiinAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiinAIService> _logger;

        private static readonly string[] OffTopicKeywords =
        {
            "movie", "film", "নাটক", "সিনেমা", "গান", "music",
            "cricket", "football", "খেলা", "game",
            "politics", "রাজনীতি", "love", "প্রেম",
            "girlfriend", "boyfriend", "sex", "cooking",
            "recipe", "রান্না", "stock", "share market",
            "crypto", "hack", "হ্যাক", "joke", "funny",
            "entertainment", "gossip", "weather", "আবহাওয়া",
            "tiktok", "youtube", "instagram", "facebook",
            "python", "javascript", "react", "programming",
            "coding", "visa", "passport"
        };

        public DiinAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<DiinAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public bool IsOffTopic(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            var lower = query.ToLowerInvariant();

            return OffTopicKeywords.Any(
                k => lower.Contains(k.ToLowerInvariant())
            );
        }

        public async Task<(bool IsHealthy, string Details)> CheckHealthAsync()
        {
            var backendUrl =
                _configuration["DIIN_AI_BACKEND_URL"]
                ?? _configuration["DiinAI:BackendUrl"]
                ?? _configuration["AI_BACKEND_URL"]
                ?? _configuration["AISettings:BackendUrl"];

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                return (false, "Backend URL is not configured in appsettings.json or environment variables.");
            }

            try
            {
                var endpoint = $"{backendUrl.TrimEnd('/')}/health";
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);
                requestMessage.Headers.Add("ngrok-skip-browser-warning", "true");
                requestMessage.Headers.Add("User-Agent", "AdDiin-NetCore-Client");

                using var response = await _httpClient.SendAsync(requestMessage);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, content);
                }

                return (false, $"HTTP {(int)response.StatusCode} {response.StatusCode}: {content}");
            }
            catch (Exception ex)
            {
                return (false, $"Connection Exception: {ex.Message}");
            }
        }

        public async Task<(string Answer, List<DiinAISource> Sources)>
            AskIslamicQuestionAsync(
                string question,
                List<DiinAIChatMessage>? history = null
            )
        {
            // 1. Guard against empty question
            if (string.IsNullOrWhiteSpace(question))
            {
                return (
                    "দয়া করে একটি প্রশ্ন লিখুন।",
                    new List<DiinAISource>()
                );
            }

            // 2. Off-Topic Guard
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

            // 3. Resolve Backend URL
            var backendUrl =
                _configuration["DIIN_AI_BACKEND_URL"]
                ?? _configuration["DiinAI:BackendUrl"]
                ?? _configuration["AI_BACKEND_URL"]
                ?? _configuration["AISettings:BackendUrl"];

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                _logger.LogWarning("Diin AI Backend URL is not configured.");
                return (
                    "দুঃখিত, AI সার্ভার কনফিগারেশন পাওয়া যায়নি। অনুগ্রহ করে appsettings.json এ DiinAI:BackendUrl সেট করুন।",
                    new List<DiinAISource>()
                );
            }

            // 4. Send request to remote Colab FastAPI
            try
            {
                var endpoint = $"{backendUrl.TrimEnd('/')}/ask";

                // We send both 'question' and 'query' in the JSON body so any backend format accepts it seamlessly
                var payload = new
                {
                    question = question.Trim(),
                    query = question.Trim()
                };

                _logger.LogInformation("Sending question to Diin AI backend: {Endpoint}", endpoint);

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(payload)
                };

                // Required headers for ngrok tunnels and standard REST APIs
                requestMessage.Headers.Add("ngrok-skip-browser-warning", "true");
                requestMessage.Headers.Add("User-Agent", "AdDiin-NetCore-Client");

                using var response = await _httpClient.SendAsync(requestMessage);

                _logger.LogInformation("Diin AI backend response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<AIApiAskResponse>(responseJson, jsonOptions);

                    if (result != null && !string.IsNullOrWhiteSpace(result.Answer))
                    {
                        _logger.LogInformation("Diin AI answer received successfully.");
                        return (result.Answer, result.Sources ?? new List<DiinAISource>());
                    }

                    _logger.LogWarning("Diin AI backend returned empty answer field. Raw body: {Body}", responseJson);
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Diin AI backend returned HTTP {StatusCode}. Response: {Response}", response.StatusCode, errorBody);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while connecting to Diin AI backend at {BackendUrl}", backendUrl);
                return (
                    "দুঃখিত, AI সার্ভার থেকে উত্তর পেতে বেশি সময় লাগছে। অনুগ্রহ করে আবার চেষ্টা করুন।",
                    new List<DiinAISource>()
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Diin AI backend at {BackendUrl}. Message: {Message}", backendUrl, ex.Message);
                return (
                    "দুঃখিত, AI সার্ভারের সাথে সংযোগ স্থাপন করা যাচ্ছে না। অনুগ্রহ করে নিশ্চিত করুন যে Colab AI সার্ভার ও ngrok চালু আছে।",
                    new List<DiinAISource>()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DiinAIService while calling {BackendUrl}", backendUrl);
            }

            return (
                "দুঃখিত, AI সার্ভারের সাথে সংযোগ স্থাপন করা যাচ্ছে না। অনুগ্রহ করে পরে আবার চেষ্টা করুন।",
                new List<DiinAISource>()
            );
        }
    }
}