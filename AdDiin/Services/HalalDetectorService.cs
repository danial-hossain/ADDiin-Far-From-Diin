using System.Net.Http.Json;
using System.Text.Json;
using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    /// <summary>
    /// Provides the hadith scheduled for the current Bangladesh time slot and
    /// creates a missing slot through the configured AI provider.
    /// </summary>
    public interface IHadithService
    {
        Task<ScheduledHadith?> GetCurrentHadithAsync(CancellationToken cancellationToken = default);
        Task EnsureCurrentHadithAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Coordinates scheduled-hadith persistence, slot selection, and generation.
    /// </summary>
    public sealed class HadithService : IHadithService
    {
        private static readonly TimeSpan[] SlotTimes =
        {
            new(8, 0, 0),
            new(14, 0, 0),
            new(20, 0, 0)
        };

        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HadithService> _logger;

        public HadithService(
            ApplicationDbContext context,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<HadithService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Reads the current slot without tracking the entity for an update.
        /// </summary>
        public async Task<ScheduledHadith?> GetCurrentHadithAsync(
            CancellationToken cancellationToken = default)
        {
            var now = GetBangladeshNow();
            var slot = GetCurrentSlot(now);

            if (slot == null)
            {
                return null;
            }

            try
            {
                return await _context.ScheduledHadiths
                    .AsNoTracking()
                    .Where(h =>
                        h.SlotDate == now.Date &&
                        h.SlotTime == slot.Value)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                _logger.LogWarning(
                    ex,
                    "ScheduledHadiths table is not available yet.");

                return null;
            }
        }

        /// <summary>
        /// Ensures the current slot has one record while tolerating concurrent
        /// application instances attempting the same first-write operation.
        /// </summary>
        public async Task EnsureCurrentHadithAsync(
            CancellationToken cancellationToken = default)
        {
            var now = GetBangladeshNow();
            var slot = GetCurrentSlot(now);

            if (slot == null)
            {
                return;
            }

            var exists = await _context.ScheduledHadiths
                .AnyAsync(
                    h =>
                        h.SlotDate == now.Date &&
                        h.SlotTime == slot.Value,
                    cancellationToken);

            if (exists)
            {
                return;
            }

            var hadith = await GenerateHadithAsync(cancellationToken);

            if (hadith == null)
            {
                return;
            }

            _context.ScheduledHadiths.Add(new ScheduledHadith
            {
                SlotDate = now.Date,
                SlotTime = slot.Value,
                Text = hadith.Value.Text,
                Source = hadith.Value.Source
            });

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Hadith slot was created by another application instance.");

                _context.ChangeTracker.Clear();
            }
        }

        // Keep provider parsing here so storage and scheduling code do not depend
        // on the external response shape.
        private async Task<(string Text, string? Source)?> GenerateHadithAsync(
            CancellationToken cancellationToken)
        {
            var apiKey =
                _configuration["GEMINI_API_KEY"]
                ?? _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning(
                    "Gemini API key is not configured. Set GEMINI_API_KEY.");

                return null;
            }

            var model =
                _configuration["Gemini:Model"]
                ?? "gemini-3.6-flash";

            var endpoint =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text =
                                    "Return one authentic hadith only as JSON with exactly these fields: text and source. " +
                                    "Use a well-known Sahih al-Bukhari or Sahih Muslim hadith, do not invent wording or attribution, " +
                                    "and do not include markdown. The text may be in English."
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    temperature = 0.2
                }
            };

            try
            {
                using var response =
                    await _httpClient.PostAsJsonAsync(
                        endpoint,
                        payload,
                        cancellationToken);

                var body =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Gemini hadith request failed with HTTP {StatusCode}: {Body}",
                        response.StatusCode,
                        body);

                    return null;
                }

                using var document = JsonDocument.Parse(body);

                var text = document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogError(
                        "Gemini returned an empty hadith response.");

                    return null;
                }

                using var hadithDocument =
                    JsonDocument.Parse(text);

                var hadithText =
                    hadithDocument.RootElement
                        .GetProperty("text")
                        .GetString();

                var source =
                    hadithDocument.RootElement
                        .GetProperty("source")
                        .GetString();

                if (string.IsNullOrWhiteSpace(hadithText) ||
                    string.IsNullOrWhiteSpace(source))
                {
                    _logger.LogError(
                        "Gemini returned an incomplete hadith payload.");

                    return null;
                }

                return (
                    hadithText.Trim(),
                    source.Trim()
                );
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Gemini returned invalid JSON for the hadith.");

                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Could not connect to Gemini for the scheduled hadith.");

                return null;
            }
        }

        // Slots are evaluated in ascending order; the latest elapsed slot is the
        // one displayed until the next scheduled slot becomes active.
        private static TimeSpan? GetCurrentSlot(DateTime now)
        {
            return SlotTimes.LastOrDefault(
                slot => now.TimeOfDay >= slot) is var slot &&
                slot != default
                    ? slot
                    : null;
        }

        private static DateTime GetBangladeshNow()
        {
            var timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    OperatingSystem.IsWindows()
                        ? "Bangladesh Standard Time"
                        : "Asia/Dhaka");

            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                timeZone);
        }

        /*
         * ================================================================
         * FEATURE BRANCH CODE — INACTIVE / REFERENCE ONLY
         * ================================================================
         *
         * The feature/daily-hadith-rotation implementation is intentionally
         * disabled because the testing/main implementation above uses the
         * ScheduledHadith database table and Gemini-based generation.
         *
         * The old implementation fetched random Bukhari hadiths directly
         * from the external Hadith API and used IMemoryCache.
         *
         * It is preserved below only as commented reference code.
         *
         * ----------------------------------------------------------------
         *
         * using System.Security.Cryptography;
         * using System.Text;
         * using System.Text.Json.Serialization;
         * using AdDiin.Models;
         * using Microsoft.Extensions.Caching.Memory;
         *
         * public interface IHadithService
         * {
         *     Task<IReadOnlyList<Hadith>> GetDailyHadithsAsync(
         *         CancellationToken cancellationToken = default);
         * }
         *
         * public class HadithService : IHadithService
         * {
         *     private const string EditionName = "eng-bukhari";
         *     private const int EditionHadithCount = 7563;
         *
         *     private readonly HttpClient _httpClient;
         *     private readonly IMemoryCache _cache;
         *     private readonly ILogger<HadithService> _logger;
         *
         *     public HadithService(
         *         HttpClient httpClient,
         *         IMemoryCache cache,
         *         ILogger<HadithService> logger)
         *     {
         *         _httpClient = httpClient;
         *         _cache = cache;
         *         _logger = logger;
         *     }
         *
         *     public async Task<IReadOnlyList<Hadith>>
         *         GetDailyHadithsAsync(
         *             CancellationToken cancellationToken = default)
         *     {
         *         var now = DateTime.UtcNow;
         *         var date = now.Date;
         *         var slotIndex = now.Hour / 6;
         *         var nextBoundary =
         *             date.AddHours((slotIndex + 1) * 6);
         *         var cacheKey =
         *             $"DailyHadith_{date:yyyyMMdd}_{slotIndex}";
         *
         *         if (_cache.TryGetValue(
         *                 cacheKey,
         *                 out IReadOnlyList<Hadith>? cachedHadiths) &&
         *             cachedHadiths is not null)
         *         {
         *             return cachedHadiths;
         *         }
         *
         *         var dailyHadiths =
         *             await FetchDailyHadithsAsync(
         *                 date,
         *                 slotIndex,
         *                 cancellationToken);
         *
         *         if (dailyHadiths.Count > 0)
         *         {
         *             _cache.Set(
         *                 cacheKey,
         *                 dailyHadiths,
         *                 new MemoryCacheEntryOptions
         *                 {
         *                     AbsoluteExpiration = nextBoundary
         *                 });
         *         }
         *
         *         return dailyHadiths;
         *     }
         *
         *     private async Task<IReadOnlyList<Hadith>>
         *         FetchDailyHadithsAsync(
         *             DateTime date,
         *             int slotIndex,
         *             CancellationToken cancellationToken)
         *     {
         *         var dailySeed = $"{date:yyyyMMdd}";
         *         var hadithNumbers = new List<int>();
         *         var seedIndex = 0;
         *
         *         while (hadithNumbers.Count < 3)
         *         {
         *             var hadithNumber =
         *                 GetHadithNumber(
         *                     $"{dailySeed}-base-{seedIndex++}");
         *
         *             if (!hadithNumbers.Contains(hadithNumber))
         *             {
         *                 hadithNumbers.Add(hadithNumber);
         *             }
         *         }
         *
         *         var replacementIndex = slotIndex % 3;
         *         var replacementNumber =
         *             GetHadithNumber(
         *                 $"{dailySeed}-slot-{slotIndex}");
         *
         *         while (
         *             hadithNumbers
         *                 .Where((_, index) => index != replacementIndex)
         *                 .Contains(replacementNumber))
         *         {
         *             replacementNumber =
         *                 GetHadithNumber(
         *                     $"{dailySeed}-slot-{slotIndex}-retry-{seedIndex++}");
         *         }
         *
         *         hadithNumbers[replacementIndex] = replacementNumber;
         *
         *         var hadiths = new List<Hadith>();
         *
         *         foreach (var hadithNumber in hadithNumbers)
         *         {
         *             try
         *             {
         *                 var response =
         *                     await _httpClient.GetFromJsonAsync<
         *                         HadithApiResponse>(
         *                         $"editions/{EditionName}/{hadithNumber}.min.json",
         *                         cancellationToken);
         *
         *                 var apiHadith =
         *                     response?.Hadiths?.FirstOrDefault();
         *
         *                 if (apiHadith == null ||
         *                     string.IsNullOrWhiteSpace(apiHadith.Text))
         *                 {
         *                     continue;
         *                 }
         *
         *                 hadiths.Add(new Hadith
         *                 {
         *                     HadithNumber =
         *                         apiHadith.HadithNumber,
         *                     Text = apiHadith.Text,
         *                     Collection = EditionName,
         *                     Reference =
         *                         $"{EditionName} #{apiHadith.Reference?.Hadith ?? apiHadith.HadithNumber}"
         *                 });
         *             }
         *             catch (Exception exception)
         *                 when (
         *                     exception is HttpRequestException or
         *                     TaskCanceledException or
         *                     JsonException)
         *             {
         *                 _logger.LogWarning(
         *                     exception,
         *                     "Could not fetch daily Hadith {HadithNumber}.",
         *                     hadithNumber);
         *             }
         *         }
         *
         *         return hadiths;
         *     }
         *
         *     private static int GetHadithNumber(string seedText)
         *     {
         *         var hash =
         *             SHA256.HashData(
         *                 Encoding.UTF8.GetBytes(seedText));
         *
         *         return new Random(
         *             BitConverter.ToInt32(hash, 0))
         *             .Next(1, EditionHadithCount + 1);
         *     }
         *
         *     private sealed class HadithApiResponse
         *     {
         *         [JsonPropertyName("hadiths")]
         *         public List<HadithApiItem> Hadiths { get; set; } = new();
         *     }
         *
         *     private sealed class HadithApiItem
         *     {
         *         [JsonPropertyName("hadithnumber")]
         *         public int HadithNumber { get; set; }
         *
         *         [JsonPropertyName("text")]
         *         public string Text { get; set; } = string.Empty;
         *
         *         [JsonPropertyName("reference")]
         *         public HadithReference? Reference { get; set; }
         *     }
         *
         *     private sealed class HadithReference
         *     {
         *         [JsonPropertyName("hadith")]
         *         public int Hadith { get; set; }
         *     }
         * }
         *
         * ================================================================
         */
    }

    public sealed class HadithSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HadithSchedulerService> _logger;

        public HadithSchedulerService(
            IServiceScopeFactory scopeFactory,
            ILogger<HadithSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var service =
                        scope.ServiceProvider
                            .GetRequiredService<IHadithService>();

                    await service.EnsureCurrentHadithAsync(
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Scheduled hadith generation failed.");
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }
    }
}