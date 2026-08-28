using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class IslamicCalendarController : Controller
    {
        private readonly IIslamicEventService _eventService;

        public IslamicCalendarController(IIslamicEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllEventsAsync();
            var today = DateTime.Today;

            // Approximate Hijri calculations for Dhaka / South Asia timezone (1447 AH)
            var days = new List<HijriCalendarDay>();
            int currentDay = today.Day;

            for (int i = 1; i <= 30; i++)
            {
                var dayDate = new DateTime(today.Year, today.Month, Math.Min(i, DateTime.DaysInMonth(today.Year, today.Month)));
                var hijriDay = (i + 14) % 30 + 1; // Sample offset for current Hijri calendar month
                var occasion = events.FirstOrDefault(e => e.EventDate.Date == dayDate.Date);

                days.Add(new HijriCalendarDay
                {
                    DayNumber = i,
                    HijriDay = hijriDay,
                    HijriMonth = "Safar / Rabi al-Awwal 1448 AH",
                    IsToday = (i == currentDay),
                    HasOccasion = occasion != null,
                    OccasionTitle = occasion?.EventName,
                    IsWhiteDay = (hijriDay == 13 || hijriDay == 14 || hijriDay == 15),
                    IsSunnahFasting = (dayDate.DayOfWeek == DayOfWeek.Monday || dayDate.DayOfWeek == DayOfWeek.Thursday)
                });
            }

            var vm = new IslamicCalendarPageViewModel
            {
                CurrentHijriDateString = "14 Safar 1448 AH",
                CurrentGregorianDateString = today.ToString("dddd, MMMM dd, yyyy"),
                CurrentHijriMonthName = "Safar 1448 AH",
                CurrentHijriYear = 1448,
                UpcomingEvents = events,
                CalendarDays = days
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetEventReminder([FromServices] INotificationService notificationService, [FromServices] Microsoft.AspNetCore.Identity.UserManager<AdDiin.Models.Entities.ApplicationUser> userManager, int eventId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Please login first to set reminders.", requireLogin = true });
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found." });

            var ev = await _eventService.GetByIdAsync(eventId);
            if (ev == null) return Json(new { success = false, message = "Islamic event not found." });

            var title = $"Reminder: {ev.EventName}";
            var message = $"Upcoming Islamic occasion on {ev.EventDate:MMMM dd, yyyy} ({ev.HijriDate}). {ev.Description}";

            await notificationService.CreateNotificationAsync(user.Id, title, message, "event", "/islamic-calendar");

            return Json(new { success = true, message = $"Reminder set for {ev.EventName}!" });
        }
    }
}
