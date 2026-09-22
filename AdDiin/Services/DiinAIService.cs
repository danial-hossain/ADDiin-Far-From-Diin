using AdDiin.Models.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdDiin.Services
{
    /// <summary>
    /// Provides the application boundary for health checks and Islamic question
    /// requests sent to the separately hosted AI backend.
    /// </summary>
    public interface IDiinAIService
    {
        Task<(string Answer, List<DiinAISource> Sources)> AskIslamicQuestionAsync(
            string question,
            List<DiinAIChatMessage>? history = null
        );

        Task<(bool IsHealthy, string Details)> CheckHealthAsync();

        bool IsOffTopic(string query);
    }

    /// <summary>
    /// Applies local request guards before translating calls to the remote AI API.
    /// </summary>
    public class DiinAIService : IDiinAIService
    {
        public const string ContactFallbackMarker = "[CONTACT_ADMIN:/contact]";
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

        /// <summary>
        /// Checks the lightweight local keyword guard used before remote requests.
        /// </summary>
        public bool IsOffTopic(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            var lower = query.ToLowerInvariant();

            return OffTopicKeywords.Any(
                k => lower.Contains(k.ToLowerInvariant())
            );
        }

        public static bool IsContactFallback(string? answer)
        {
            return !string.IsNullOrWhiteSpace(answer) &&
                answer.Contains(ContactFallbackMarker, StringComparison.OrdinalIgnoreCase);
        }

        private static string ContactFallback(string message)
        {
            return $"{message} সহায়তার জন্য আমাদের সাথে যোগাযোগ করুন: {ContactFallbackMarker}";
        }

        /// <summary>
        /// Queries the backend health endpoint without creating a chat request.
        /// </summary>
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

        /// <summary>
        /// Validates the question, sends recent history, and normalizes the
        /// backend response into the view model shape used by the UI.
        /// </summary>
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
                    ContactFallback("দুঃখিত, AI সার্ভার কনফিগারেশন পাওয়া যায়নি। অনুগ্রহ করে appsettings.json এ DiinAI:BackendUrl সেট করুন।"),
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
                    query = question.Trim(),
                    history = history?.TakeLast(20).Select(message => new
                    {
                        role = message.Role,
                        content = message.Content
                    })
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
                    using var document = JsonDocument.Parse(responseJson);
                    var root = document.RootElement;
                    var answer = ReadString(root, "answer", "response", "message");

                    if (!string.IsNullOrWhiteSpace(answer))
                    {
                        _logger.LogInformation("Diin AI answer received successfully.");
                        return (answer, ReadSources(root));
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
                    ContactFallback("দুঃখিত, AI সার্ভার থেকে উত্তর পেতে বেশি সময় লাগছে। অনুগ্রহ করে আবার চেষ্টা করুন।"),
                    new List<DiinAISource>()
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Diin AI backend at {BackendUrl}. Message: {Message}", backendUrl, ex.Message);
                return (
                    ContactFallback("দুঃখিত, AI সার্ভারের সাথে সংযোগ স্থাপন করা যাচ্ছে না। অনুগ্রহ করে নিশ্চিত করুন যে Colab AI সার্ভার ও ngrok চালু আছে।"),
                    new List<DiinAISource>()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DiinAIService while calling {BackendUrl}", backendUrl);
            }

            return (
                ContactFallback("দুঃখিত, AI সার্ভারের সাথে সংযোগ স্থাপন করা যাচ্ছে না। অনুগ্রহ করে পরে আবার চেষ্টা করুন।"),
                new List<DiinAISource>()
            );
        }

        private static string? ReadString(JsonElement element, params string[] names)
        {
            foreach (var name in names)
            {
                if (element.TryGetProperty(name, out var value) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }

                var property = element.EnumerateObject()
                    .FirstOrDefault(property => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase));
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    return property.Value.GetString();
                }
            }

            return null;
        }

        private static List<DiinAISource> ReadSources(JsonElement root)
        {
            var sourceArray = FindSourceArray(root);

            if (sourceArray.ValueKind != JsonValueKind.Array)
            {
                return new List<DiinAISource>();
            }

            var sources = new List<DiinAISource>();
            foreach (var item in sourceArray.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var reference = item.GetString();
                    if (!string.IsNullOrWhiteSpace(reference))
                    {
                        sources.Add(new DiinAISource
                        {
                            Source = "Knowledge Base",
                            Reference = reference
                        });
                    }

                    continue;
                }

                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var id = ReadString(item, "id", "key");
                var source = ReadString(item, "source", "name", "title", "document", "file", "filename");
                var itemReference = ReadString(item, "reference", "citation", "url", "link", "page", "locator", "content");
                var text = ReadString(item, "text", "content", "page_content", "pageContent", "excerpt");

                if (!string.IsNullOrWhiteSpace(source) ||
                    !string.IsNullOrWhiteSpace(itemReference) ||
                    !string.IsNullOrWhiteSpace(text))
                {
                    sources.Add(new DiinAISource
                    {
                        Id = id ?? string.Empty,
                        Source = string.IsNullOrWhiteSpace(source) ? "Knowledge Base" : source,
                        Reference = itemReference ?? string.Empty,
                        Text = text ?? string.Empty
                    });
                }
            }

            return sources;
        }

        private static JsonElement FindSourceArray(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var sourceNames = new[]
                {
                    "sources", "references", "citations", "documents",
                    "knowledge_base_sources", "knowledgeBaseSources",
                    "retrieved_documents", "retrievedDocuments", "context"
                };

                foreach (var property in element.EnumerateObject())
                {
                    if (sourceNames.Any(name => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)) &&
                        property.Value.ValueKind == JsonValueKind.Array)
                    {
                        return property.Value;
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    var nested = FindSourceArray(property.Value);
                    if (nested.ValueKind == JsonValueKind.Array)
                    {
                        return nested;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object &&
                        (ReadString(item, "source", "reference", "text", "page_content") != null))
                    {
                        return element;
                    }
                }
            }

            return default;
        }
    }
}