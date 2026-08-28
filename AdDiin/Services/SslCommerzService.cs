using System.Text.Json;
using System.Text.Json.Serialization;
using AdDiin.Models.Entities;

namespace AdDiin.Services
{
    public class SslCommerzInitResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("failedreason")]
        public string? FailedReason { get; set; }

        [JsonPropertyName("sessionkey")]
        public string? SessionKey { get; set; }

        [JsonPropertyName("GatewayPageURL")]
        public string? GatewayPageURL { get; set; }
    }

    public class SslCommerzValidationResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("tran_id")]
        public string? TranId { get; set; }

        [JsonPropertyName("val_id")]
        public string? ValId { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("card_type")]
        public string? CardType { get; set; }

        [JsonPropertyName("bank_tran_id")]
        public string? BankTranId { get; set; }
    }

    public interface ISslCommerzService
    {
        Task<SslCommerzInitResponse?> InitiatePaymentAsync(Donation donation, string hostUrl);
        Task<bool> ValidatePaymentAsync(string valId);
    }

    public class SslCommerzService : ISslCommerzService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SslCommerzService> _logger;

        private readonly string _storeId;
        private readonly string _storePassword;
        private readonly bool _isTestMode;
        private readonly string _baseUrl;

        public SslCommerzService(IConfiguration config, ILogger<SslCommerzService> logger)
        {
            _config = config;
            _logger = logger;
            _httpClient = new HttpClient();

            _storeId = _config["SslCommerzSettings:StoreId"] ?? "techb69a9bffeeaf40";
            _storePassword = _config["SslCommerzSettings:StorePassword"] ?? "techb69a9bffeeaf40@ssl";
            _isTestMode = bool.Parse(_config["SslCommerzSettings:IsTestMode"] ?? "true");
            var sandboxUrl = _config["SslCommerzSettings:SandboxUrl"] ?? "https://sandbox.sslcommerz.com";
            var liveUrl = _config["SslCommerzSettings:LiveUrl"] ?? "https://securepay.sslcommerz.com";

            _baseUrl = _isTestMode ? sandboxUrl : liveUrl;
        }

        public async Task<SslCommerzInitResponse?> InitiatePaymentAsync(Donation donation, string hostUrl)
        {
            try
            {
                var initEndpoint = $"{_baseUrl.TrimEnd('/')}/gwprocess/v4/api.php";

                var categoryTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "zakat", "Zakat Fund" },
                    { "orphan", "Orphan Care & Student Sponsorship" },
                    { "sitarto", "Winter Clothes Fund" },
                    { "iftar", "Ramadan Food & Iftar" },
                    { "durjog", "Emergency Disaster & Flood Relief" },
                    { "gachropon", "Tree Plantation Sadaqah" },
                    { "kurbani", "Qurbani Meat Distribution" },
                    { "mosque", "Mosque Development Fund" },
                    { "education", "Quran & Madrasa Education" },
                    { "medical", "Emergency Medical Aid" },
                    { "general", "General Sadaqah" }
                };

                var catName = categoryTitles.TryGetValue(donation.Category, out var title) ? title : "General Donation";

                var values = new Dictionary<string, string>
                {
                    { "store_id", _storeId },
                    { "store_passwd", _storePassword },
                    { "total_amount", donation.Amount.ToString("0.00") },
                    { "currency", "BDT" },
                    { "tran_id", donation.TranId },
                    { "success_url", $"{hostUrl.TrimEnd('/')}/Donate/SslSuccess" },
                    { "fail_url", $"{hostUrl.TrimEnd('/')}/Donate/SslFail" },
                    { "cancel_url", $"{hostUrl.TrimEnd('/')}/Donate/SslCancel" },
                    { "ipn_url", $"{hostUrl.TrimEnd('/')}/Donate/SslIpn" },
                    { "cus_name", string.IsNullOrWhiteSpace(donation.Name) ? "ADDiin Generous Donor" : donation.Name },
                    { "cus_email", string.IsNullOrWhiteSpace(donation.Email) ? "donor@addiin.com" : donation.Email },
                    { "cus_phone", string.IsNullOrWhiteSpace(donation.Phone) ? "01700000000" : donation.Phone },
                    { "cus_add1", "Dhaka, Bangladesh" },
                    { "cus_city", "Dhaka" },
                    { "cus_country", "Bangladesh" },
                    { "shipping_method", "NO" },
                    { "product_name", $"ADDiin {catName}" },
                    { "product_category", "Donation" },
                    { "product_profile", "non-physical-goods" },
                    { "value_a", donation.Category },
                    { "value_b", donation.UserId?.ToString() ?? "" }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await _httpClient.PostAsync(initEndpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("SSLCommerz Init Response: {Response}", responseString);

                var result = JsonSerializer.Deserialize<SslCommerzInitResponse>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz Initiation Failed");
                return null;
            }
        }

        public async Task<bool> ValidatePaymentAsync(string valId)
        {
            if (string.IsNullOrEmpty(valId)) return false;

            try
            {
                var validateEndpoint = $"{_baseUrl.TrimEnd('/')}/validator/api/validationserverAPI.php?val_id={valId}&store_id={_storeId}&store_passwd={_storePassword}&v=1&format=json";
                var response = await _httpClient.GetAsync(validateEndpoint);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("SSLCommerz Validation Response: {Response}", responseString);

                var result = JsonSerializer.Deserialize<SslCommerzValidationResponse>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Status?.Equals("VALID", StringComparison.OrdinalIgnoreCase) == true ||
                       result?.Status?.Equals("VALIDATED", StringComparison.OrdinalIgnoreCase) == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz Validation Failed");
                return false;
            }
        }
    }
}
