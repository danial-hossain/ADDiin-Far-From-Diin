using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    [Authorize]
    public class MyDeenController : Controller
    {
        private readonly IMyDeenService _myDeenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public MyDeenController(
            IMyDeenService myDeenService,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _myDeenService = myDeenService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _notificationService.SeedDefaultRemindersAsync(user.Id);
            var hubData = await _myDeenService.GetHubDataAsync(user.Id);
            return View(hubData);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleGoal(string goalName, bool isCompleted)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var updated = await _myDeenService.ToggleGoalItemAsync(user.Id, goalName, isCompleted);
            var settings = await _myDeenService.GetOrCreateSettingsAsync(user.Id);

            return Json(new
            {
                success = true,
                percentage = updated.CompletionPercentage,
                streak = settings.CurrentStreak
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveDhikr(string dhikrName, int count, int target)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var record = await _myDeenService.SaveDhikrAsync(user.Id, dhikrName, count, target);
            return Json(new
            {
                success = true,
                count = record.Count,
                target = record.TargetCount,
                isAchieved = record.IsTargetAchieved
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuran(string goalType, int target, string surah, int ayah, int pagesRead)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var log = await _myDeenService.SaveQuranProgressAsync(user.Id, goalType, target, surah, ayah, pagesRead);
            return Json(new
            {
                success = true,
                pagesRead = log.PagesReadToday,
                isCompleted = log.IsCompleted
            });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdhkar(string adhkarType, string itemKey, string title, bool isCompleted)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _myDeenService.ToggleAdhkarItemAsync(user.Id, adhkarType, itemKey, title, isCompleted);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRuqyah(string routineType, string itemKey, string title, bool isCompleted)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _myDeenService.ToggleRuqyahItemAsync(user.Id, routineType, itemKey, title, isCompleted);
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(UserDeenSettings model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _myDeenService.UpdateSettingsAsync(user.Id, model);
            TempData["SuccessMessage"] = "My Deen preferences and reminder settings updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
