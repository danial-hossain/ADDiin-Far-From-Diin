using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class PrayerTimesController : Controller
    {
        private readonly IPrayerTimeService _prayerService;

        public PrayerTimesController(IPrayerTimeService prayerService)
        {
            _prayerService = prayerService;
        }

        public async Task<IActionResult> Index()
        {
            var prayers = await _prayerService.GetAllAsync();
            var (nextPrayer, timeRemaining) = await _prayerService.GetNextPrayerAsync();

            ViewBag.NextPrayer = nextPrayer;
            ViewBag.TimeRemaining = timeRemaining;

            return View(prayers);
        }

        [HttpGet]
        public async Task<IActionResult> LiveTimes()
        {
            var prayers = await _prayerService.GetAllAsync();
            var (nextPrayer, timeRemaining) = await _prayerService.GetNextPrayerAsync();

            return Json(new
            {
                success = true,
                prayers = prayers.Select(p => new
                {
                    p.Id,
                    p.PrayerName,
                    time = p.PrayerTimeValue.ToString(@"hh\:mm"),
                    p.DisplayNameEn,
                    p.DisplayNameBn,
                    p.Category,
                    p.PrayerType,
                    p.DisplayOrder
                }),
                nextPrayer = nextPrayer == null ? null : new
                {
                    nextPrayer.DisplayNameEn,
                    nextPrayer.DisplayNameBn,
                    time = nextPrayer.PrayerTimeValue.ToString(@"hh\:mm"),
                    secondsRemaining = (int)timeRemaining.TotalSeconds
                }
            });
        }
    }
}
