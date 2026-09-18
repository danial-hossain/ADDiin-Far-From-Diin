using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdDiin.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AdDiin.Services
{
    public interface IHadithService
    {
        Task<IReadOnlyList<Hadith>> GetDailyHadithsAsync(CancellationToken cancellationToken = default);
    }

    public class HadithService : IHadithService
    {
        private const string EditionName = "eng-bukhari";
        private const int EditionHadithCount = 7563;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HadithService> _logger;

        public HadithService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<HadithService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Hadith>> GetDailyHadithsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var date = now.Date;
            var slotIndex = now.Hour / 6;
            var nextBoundary = date.AddHours((slotIndex + 1) * 6);
            var cacheKey = $"DailyHadith_{date:yyyyMMdd}_{slotIndex}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Hadith>? cachedHadiths) && cachedHadiths is not null)
            {
                return cachedHadiths;
            }

            var dailyHadiths = await FetchDailyHadithsAsync(date, slotIndex, cancellationToken);
            if (dailyHadiths.Count > 0)
            {
                _cache.Set(cacheKey, dailyHadiths, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = nextBoundary
                });
            }

            return dailyHadiths;
        }

        private async Task<IReadOnlyList<Hadith>> FetchDailyHadithsAsync(
            DateTime date,
            int slotIndex,
            CancellationToken cancellationToken)
        {
            var dailySeed = $"{date:yyyyMMdd}";
            var hadithNumbers = new List<int>();
            var seedIndex = 0;
            while (hadithNumbers.Count < 3)
            {
                var hadithNumber = GetHadithNumber($"{dailySeed}-base-{seedIndex++}");
                if (!hadithNumbers.Contains(hadithNumber))
                {
                    hadithNumbers.Add(hadithNumber);
                }
            }

            var replacementIndex = slotIndex % 3;
            var replacementNumber = GetHadithNumber($"{dailySeed}-slot-{slotIndex}");
            while (hadithNumbers.Where((_, index) => index != replacementIndex).Contains(replacementNumber))
            {
                replacementNumber = GetHadithNumber($"{dailySeed}-slot-{slotIndex}-retry-{seedIndex++}");
            }

            hadithNumbers[replacementIndex] = replacementNumber;

            var hadiths = new List<Hadith>();
            foreach (var hadithNumber in hadithNumbers)
            {
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<HadithApiResponse>(
                        $"editions/{EditionName}/{hadithNumber}.min.json",
                        cancellationToken);

                    var apiHadith = response?.Hadiths?.FirstOrDefault();
                    if (apiHadith == null || string.IsNullOrWhiteSpace(apiHadith.Text))
                    {
                        continue;
                    }

                    hadiths.Add(new Hadith
                    {
                        HadithNumber = apiHadith.HadithNumber,
                        Text = apiHadith.Text,
                        Collection = EditionName,
                        Reference = $"{EditionName} #{apiHadith.Reference?.Hadith ?? apiHadith.HadithNumber}"
                    });
                }
                catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
                {
                    _logger.LogWarning(exception, "Could not fetch daily Hadith {HadithNumber}.", hadithNumber);
                }
            }

            return hadiths;
        }

        private static int GetHadithNumber(string seedText)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seedText));
            return new Random(BitConverter.ToInt32(hash, 0)).Next(1, EditionHadithCount + 1);
        }

        private sealed class HadithApiResponse
        {
            [JsonPropertyName("hadiths")]
            public List<HadithApiItem> Hadiths { get; set; } = new();
        }

        private sealed class HadithApiItem
        {
            [JsonPropertyName("hadithnumber")]
            public int HadithNumber { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("reference")]
            public HadithReference? Reference { get; set; }
        }

        private sealed class HadithReference
        {
            [JsonPropertyName("hadith")]
            public int Hadith { get; set; }
        }
    }
}
