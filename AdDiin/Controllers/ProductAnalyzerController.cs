using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class ProductAnalyzerController : Controller
    {
        private readonly IHalalDetectorService _halalDetectorService;
        private readonly ILogger<ProductAnalyzerController> _logger;

        public ProductAnalyzerController(IHalalDetectorService halalDetectorService, ILogger<ProductAnalyzerController> logger)
        {
            _halalDetectorService = halalDetectorService;
            _logger = logger;
        }

        [Route("product-analyzer")]
        [Route("ProductAnalyzer")]
        public IActionResult Index()
        {
            ViewData["Title"] = "AI Halal/Haram Product Analyzer | ADDiin";
            return View();
        }

        [HttpPost]
        [Route("api/product-analyzer/analyze")]
        public async Task<IActionResult> AnalyzeProduct([FromForm] IFormFile? image, [FromForm] string? rawText)
        {
            try
            {
                _logger.LogInformation("Product analyzer request received. HasImage: {HasImage}, RawTextLength: {TextLength}", 
                    image != null, rawText?.Length ?? 0);

                var result = await _halalDetectorService.AnalyzeProductImageAsync(image, rawText);

                if (!result.Success && !string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    return BadRequest(new
                    {
                        status = "error",
                        error = result.ErrorMessage,
                        message = result.ErrorMessage
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during product analysis.");
                return StatusCode(500, new
                {
                    status = "error",
                    error = "AI analysis service is temporarily unavailable. Please try again.",
                    message = "AI analysis service is temporarily unavailable. Please try again."
                });
            }
        }

        [HttpPost]
        [Route("api/product-analyzer/analyze-text")]
        public async Task<IActionResult> AnalyzeProductText([FromBody] ProductTextAnalysisRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest(new
                    {
                        status = "error",
                        error = "Please enter ingredients text to analyze.",
                        message = "Please enter ingredients text to analyze."
                    });
                }

                _logger.LogInformation("Product analyzer text request received. TextLength: {Length}", request.Text.Length);

                var result = await _halalDetectorService.AnalyzeProductTextAsync(request.Text);

                if (!result.Success && !string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    return BadRequest(new
                    {
                        status = "error",
                        error = result.ErrorMessage,
                        message = result.ErrorMessage
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during manual text product analysis.");
                return StatusCode(500, new
                {
                    status = "error",
                    error = "AI analysis service is temporarily unavailable. Please try again.",
                    message = "AI analysis service is temporarily unavailable. Please try again."
                });
            }
        }

        [HttpGet]
        [Route("api/product-analyzer/health")]
        public async Task<IActionResult> Health()
        {
            var (isHealthy, details) = await _halalDetectorService.CheckHealthAsync();
            return Ok(new
            {
                connected = isHealthy,
                details = details,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }
    }
}
