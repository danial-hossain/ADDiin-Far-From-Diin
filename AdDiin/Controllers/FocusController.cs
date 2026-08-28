using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class FocusController : Controller
    {
        [Route("focus")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Digital Fast & Focus Shield | ADDiin";
            return View();
        }

        [Route("focus/shield")]
        public IActionResult Shield([FromQuery] string? domain = null)
        {
            ViewData["Title"] = "Spiritual Shield Active | ADDiin";
            ViewBag.BlockedDomain = string.IsNullOrWhiteSpace(domain) ? "Social Media" : domain;
            return View();
        }

        [HttpGet]
        [Route("api/focus/status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "success",
                message = "ADDiin Focus Shield is ready."
            });
        }
    }
}
