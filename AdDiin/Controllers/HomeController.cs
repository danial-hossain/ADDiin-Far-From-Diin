using AdDiin.Models;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdDiin.Controllers
{
    /// <summary>
    /// Provides public landing, informational, and error pages.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IPrayerTimeService _prayerService;
        private readonly IIslamicEventService _eventService;
        private readonly IActivityService _activityService;
        private readonly IAboutService _aboutService;

        public HomeController(
            IPrayerTimeService prayerService,
            IIslamicEventService eventService,
            IActivityService activityService,
            IAboutService aboutService)
        {
            _prayerService = prayerService;
            _eventService = eventService;
            _activityService = activityService;
            _aboutService = aboutService;
        }

        public async Task<IActionResult> Index()
        {
            // The home page composes independent service results into one view
            // model so the view does not need to know how each feature is stored.
            var jamaat = await _prayerService.GetJamaatTimesAsync();
            var azan = await _prayerService.GetAzanTimesAsync();
            var events = await _eventService.GetUpcomingEventsAsync();
            var activities = await _activityService.GetActiveActivitiesAsync();
            var (nextPrayer, timeRemaining) = await _prayerService.GetNextPrayerAsync();

            var vm = new HomeViewModel
            {
                JamaatPrayers = jamaat,
                AzanPrayers = azan,
                UpcomingEvents = events.Take(4).ToList(),
                OngoingActivities = activities.Take(3).ToList(),
                NextPrayer = nextPrayer,
                TimeUntilNextPrayer = timeRemaining
            };

            return View(vm);
        }

        public async Task<IActionResult> About()
        {
            // About content is file-backed so administrators can update it
            // without adding a database migration for editorial text.
            var content = await _aboutService.GetContentAsync();
            return View(content);
        }

        public IActionResult SDG9()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
