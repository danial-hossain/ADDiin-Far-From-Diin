using AdDiin.Models.ViewModels;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AdDiin.Services
{
    public interface IHalalDetectorService
    {
        Task<HalalDetectorResult> AnalyzeProductImageAsync(IFormFile? imageFile, string? rawText = null);
        Task<HalalDetectorResult> AnalyzeProductTextAsync(string text);
        Task<(bool IsHealthy, string Details)> CheckHealthAsync();
    }

    public class HalalDetectorService : IHalalDetectorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HalalDetectorService> _logger;

        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp", "image/jpg" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public HalalDetectorService(HttpClient httpClient, IConfiguration configuration, ILogger<HalalDetectorService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool IsHealthy, string Details)> CheckHealthAsync()
        {
            var backendUrl = _configuration["HalalDetector:BackendUrl"]
                             ?? _configuration["HALAL_DETECTOR_BACKEND_URL"]
                             ?? _configuration["AdDiinAI:BaseUrl"]
                             ?? _configuration["DiinAI:BackendUrl"];

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                return (false, "Halal Detector Backend URL is not configured in appsettings.json.");
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

        public async Task<HalalDetectorResult> AnalyzeProductImageAsync(IFormFile? imageFile, string? rawText = null)
        {
            // 1. Resolve Backend URL
            var backendUrl = _configuration["HalalDetector:BackendUrl"]
                             ?? _configuration["HALAL_DETECTOR_BACKEND_URL"]
                             ?? _configuration["AdDiinAI:BaseUrl"]
                             ?? _configuration["DiinAI:BackendUrl"];

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                _logger.LogWarning("Halal Detector Backend URL is not configured. Set HalalDetector:BackendUrl in appsettings.json.");
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "AI analysis service is temporarily unavailable. Please try again.",
                    Message = "AI analysis service is temporarily unavailable. Please try again."
                };
            }

            // 2. Validate Image File (if provided)
            if (imageFile != null)
            {
                if (imageFile.Length == 0)
                {
                    return new HalalDetectorResult
                    {
                        Success = false,
                        Status = "error",
                        ErrorMessage = "Uploaded image file is empty. Please upload a valid ingredient label image.",
                        Message = "Uploaded image file is empty. Please upload a valid ingredient label image."
                    };
                }

                if (imageFile.Length > MaxFileSizeBytes)
                {
                    return new HalalDetectorResult
                    {
                        Success = false,
                        Status = "error",
                        ErrorMessage = "Image file size exceeds the 10 MB limit.",
                        Message = "Image file size exceeds the 10 MB limit."
                    };
                }

                var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext) || (!string.IsNullOrEmpty(imageFile.ContentType) && !AllowedContentTypes.Contains(imageFile.ContentType.ToLowerInvariant())))
                {
                    return new HalalDetectorResult
                    {
                        Success = false,
                        Status = "error",
                        ErrorMessage = "Only JPG, PNG, or WEBP image formats are supported.",
                        Message = "Only JPG, PNG, or WEBP image formats are supported."
                    };
                }
            }
            else
            {
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "Please upload an image of the product ingredients label.",
                    Message = "Please upload an image of the product ingredients label."
                };
            }

            // 3. Prepare Multipart Form Data Request to Remote FastAPI AI Backend
            try
            {
                var endpoint = $"{backendUrl.TrimEnd('/')}/api/analyze-product";
                _logger.LogInformation("Forwarding product image to Halal Detector Backend: {Endpoint}", endpoint);

                using var content = new MultipartFormDataContent();
                var fileBytes = await ToByteArrayAsync(imageFile);
                var fileContent = new ByteArrayContent(fileBytes);
                
                var contentType = !string.IsNullOrWhiteSpace(imageFile.ContentType) ? imageFile.ContentType : "image/jpeg";
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(fileContent, "image", imageFile.FileName);

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = content
                };

                requestMessage.Headers.Add("ngrok-skip-browser-warning", "true");
                requestMessage.Headers.Add("User-Agent", "AdDiin-NetCore-Client");

                using var response = await _httpClient.SendAsync(requestMessage);
                _logger.LogInformation("Halal Detector Backend responded with status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var parsedResult = JsonSerializer.Deserialize<HalalDetectorResult>(jsonResponse, jsonOptions);
                    if (parsedResult != null)
                    {
                        parsedResult.Success = true;
                        if (string.IsNullOrWhiteSpace(parsedResult.Status))
                        {
                            parsedResult.Status = parsedResult.Decision?.Status ?? "NO_HARAM_MATCH";
                        }
                        return parsedResult;
                    }
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Halal detector API returned failure status code {StatusCode}: {Body}", response.StatusCode, errorBody);

                    if ((int)response.StatusCode == 400)
                    {
                        return new HalalDetectorResult
                        {
                            Success = false,
                            Status = "error",
                            ErrorMessage = "Invalid image submitted for analysis. Please upload a clear ingredient label.",
                            Message = "Invalid image submitted for analysis. Please upload a clear ingredient label."
                        };
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout occurred while connecting to Halal Detector Backend at {BackendUrl}", backendUrl);
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "The analysis is taking longer than expected. Please try again.",
                    Message = "The analysis is taking longer than expected. Please try again."
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Halal Detector Backend at {BackendUrl}", backendUrl);
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "AI analysis service is temporarily unavailable. Please try again.",
                    Message = "AI analysis service is temporarily unavailable. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error communicating with Halal Detector Backend at {BackendUrl}", backendUrl);
            }

            return new HalalDetectorResult
            {
                Success = false,
                Status = "error",
                ErrorMessage = "AI analysis service is temporarily unavailable. Please try again.",
                Message = "AI analysis service is temporarily unavailable. Please try again."
            };
        }

        public async Task<HalalDetectorResult> AnalyzeProductTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "Please enter a list of ingredients to analyze.",
                    Message = "Please enter a list of ingredients to analyze."
                };
            }

            var trimmedText = text.Trim();
            if (trimmedText.Length < 3)
            {
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "Please enter a meaningful list of ingredients.",
                    Message = "Please enter a meaningful list of ingredients."
                };
            }

            var backendUrl = _configuration["HalalDetector:BackendUrl"]
                             ?? _configuration["HALAL_DETECTOR_BACKEND_URL"]
                             ?? _configuration["AdDiinAI:BaseUrl"]
                             ?? _configuration["DiinAI:BackendUrl"];

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                _logger.LogWarning("Halal Detector Backend URL is not configured. Set HalalDetector:BackendUrl in appsettings.json.");
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "AI analysis service is temporarily unavailable. Please try again.",
                    Message = "AI analysis service is temporarily unavailable. Please try again."
                };
            }

            try
            {
                var endpoint = $"{backendUrl.TrimEnd('/')}/api/analyze-text";
                _logger.LogInformation("Forwarding manual ingredients text to Halal Detector Backend: {Endpoint}", endpoint);

                var payload = new { text = trimmedText };
                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = jsonContent
                };

                requestMessage.Headers.Add("ngrok-skip-browser-warning", "true");
                requestMessage.Headers.Add("User-Agent", "AdDiin-NetCore-Client");

                using var response = await _httpClient.SendAsync(requestMessage);
                _logger.LogInformation("Halal Detector Backend /api/analyze-text responded with status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var parsedResult = JsonSerializer.Deserialize<HalalDetectorResult>(jsonResponse, jsonOptions);
                    if (parsedResult != null)
                    {
                        parsedResult.Success = true;
                        if (string.IsNullOrWhiteSpace(parsedResult.Status))
                        {
                            parsedResult.Status = parsedResult.Decision?.Status ?? "NO_HARAM_MATCH";
                        }
                        return parsedResult;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("AI backend /api/analyze-text endpoint is not yet deployed on remote FastAPI. Falling back gracefully.");
                    return new HalalDetectorResult
                    {
                        Success = false,
                        Status = "error",
                        ErrorMessage = "The external AI backend currently supports image analysis. The text-analysis endpoint (/api/analyze-text) is being deployed. Please use the Upload Image tab in the meantime.",
                        Message = "The external AI backend currently supports image analysis. The text-analysis endpoint (/api/analyze-text) is being deployed. Please use the Upload Image tab in the meantime."
                    };
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Halal detector text API returned failure status code {StatusCode}: {Body}", response.StatusCode, errorBody);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout occurred while connecting to Halal Detector Backend at {BackendUrl}", backendUrl);
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "The analysis is taking longer than expected. Please try again.",
                    Message = "The analysis is taking longer than expected. Please try again."
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Halal Detector Backend at {BackendUrl}", backendUrl);
                return new HalalDetectorResult
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = "AI analysis service is temporarily unavailable. Please try again.",
                    Message = "AI analysis service is temporarily unavailable. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error communicating with Halal Detector Backend at {BackendUrl}", backendUrl);
            }

            return new HalalDetectorResult
            {
                Success = false,
                Status = "error",
                ErrorMessage = "AI analysis service is temporarily unavailable. Please try again.",
                Message = "AI analysis service is temporarily unavailable. Please try again."
            };
        }

        private static async Task<byte[]> ToByteArrayAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
