using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace AdDiin.Controllers
{
    public class FocusController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FocusController(IWebHostEnvironment env)
        {
            _env = env;
        }

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

        [Route("focus/download-installer")]
        public IActionResult DownloadInstaller()
        {
            var filePath = Path.Combine(_env.WebRootPath, "extension", "install-extension.bat");
            if (!System.IO.File.Exists(filePath))
            {
                var content = @"@echo off
title ADDiin Focus Shield Installer
color 0A
echo ========================================================
echo         ADDiin Focus Shield - 1-Click Extension Setup
echo ========================================================
echo.
set EXT_PATH=C:\Addin SD 5\Ad-Diin\AdDiin\wwwroot\extension
start chrome ""chrome://extensions"" 2>nul || start msedge ""edge://extensions"" 2>nul
explorer ""%EXT_PATH%""
echo ========================================================
echo  DONE! Drag the opened folder into your browser window
echo  or click 'Load unpacked' and select the folder.
echo ========================================================
pause";
                System.IO.File.WriteAllText(filePath, content);
            }

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/octet-stream", "install-extension.bat");
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
