using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        public async Task<IActionResult> Index()
        {
            var activities = await _activityService.GetActiveActivitiesAsync();
            return View(activities);
        }

        public async Task<IActionResult> Details(int id)
        {
            var activity = await _activityService.GetByIdAsync(id);
            if (activity == null) return NotFound();

            return View(activity);
        }
    }
}
