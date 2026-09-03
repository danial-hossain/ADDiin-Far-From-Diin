using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class ProductAnalyzerController : Controller
    {
        private readonly ILogger<ProductAnalyzerController> _logger;

        public ProductAnalyzerController(ILogger<ProductAnalyzerController> logger)
        {
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
                // Placeholder ready for backend OCR / LLM Vision / FastAPI integration
                _logger.LogInformation("Product analyzer request received. HasImage: {HasImage}, RawTextLength: {TextLength}", 
                    image != null, rawText?.Length ?? 0);

                await Task.Delay(50);

                return Ok(new
                {
                    status = "success",
                    message = "Product analyzed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during product analysis.");
                return StatusCode(500, new { error = "An error occurred while processing product label." });
            }
        }
    }
}
