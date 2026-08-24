using AdDiin.Models;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdDiin.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPrayerTimeService _prayerService;
        private readonly IIslamicEventService _eventService;
        private readonly IActivityService _activityService;
        private readonly IAboutService _aboutService;
        private readonly IContactService _contactService;

        public HomeController(
            IPrayerTimeService prayerService,
            IIslamicEventService eventService,
            IActivityService activityService,
            IAboutService aboutService,
            IContactService contactService)
        {
            _prayerService = prayerService;
            _eventService = eventService;
            _activityService = activityService;
            _aboutService = aboutService;
            _contactService = contactService;
        }

        public async Task<IActionResult> Index()
        {
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
            var content = await _aboutService.GetContentAsync();
            return View(content);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _contactService.SubmitMessageAsync(model);
            TempData["SuccessMessage"] = "JazakAllah Khair! Your message has been sent successfully. The Mosque administration will respond to you shortly.";
            return RedirectToAction(nameof(Contact));
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
