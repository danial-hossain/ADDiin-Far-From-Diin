using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IMyDeenService _myDeenService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            INotificationService notificationService,
            IMyDeenService myDeenService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _myDeenService = myDeenService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string category = "all")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _notificationService.SeedDefaultRemindersAsync(user.Id);
            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id, category);
            var unreadCount = await _notificationService.GetUnreadCountAsync(user.Id);
            var settings = await _myDeenService.GetOrCreateSettingsAsync(user.Id);

            var vm = new NotificationsPageViewModel
            {
                Notifications = notifications,
                UnreadCount = unreadCount,
                SelectedCategory = category,
                Settings = settings
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _notificationService.MarkAsReadAsync(id, user.Id);
            var unreadCount = await _notificationService.GetUnreadCountAsync(user.Id);

            return Json(new { success, unreadCount });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _notificationService.DeleteNotificationAsync(id, user.Id);
            var unreadCount = await _notificationService.GetUnreadCountAsync(user.Id);

            return Json(new { success, unreadCount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _notificationService.MarkAllAsReadAsync(user.Id);
            TempData["SuccessMessage"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { unreadCount = 0 });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { unreadCount = 0 });

            var unreadCount = await _notificationService.GetUnreadCountAsync(user.Id);
            return Json(new { unreadCount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePreferences(bool prayerReminder, bool calendarReminder, bool quranReminder, bool adhkarReminder)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var settings = await _myDeenService.GetOrCreateSettingsAsync(user.Id);
            settings.PrayerReminder = prayerReminder;
            settings.CalendarReminder = calendarReminder;
            settings.QuranReminder = quranReminder;
            settings.AdhkarReminder = adhkarReminder;

            await _myDeenService.UpdateSettingsAsync(user.Id, settings);
            TempData["SuccessMessage"] = "Notification preferences updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
