using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly IActivityService _activityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivitiesController(
            IActivityService activityService,
            UserManager<ApplicationUser> userManager)
        {
            _activityService = activityService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category = null, string? search = null)
        {
            var activities = await _activityService.GetActiveActivitiesAsync(category, search);
            ViewBag.SelectedCategory = category ?? "all";
            ViewBag.SearchQuery = search;
            return View(activities);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var activity = await _activityService.GetByIdAsync(id);
            if (activity == null) return NotFound();

            var registrationModel = new ProgramRegistrationInputModel
            {
                ActivityId = activity.Id,
                ActivityTitle = activity.Title
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    registrationModel.FullName = user.FullName;
                    registrationModel.Email = user.Email ?? string.Empty;
                    registrationModel.PhoneNumber = user.PhoneNumber ?? string.Empty;
                }
            }

            ViewBag.RegistrationModel = registrationModel;
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ProgramRegistrationInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required registration details accurately.";
                return RedirectToAction(nameof(Details), new { id = model.ActivityId });
            }

            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                userId = user?.Id;
            }

            var reg = await _activityService.RegisterForProgramAsync(model, userId);

            TempData["SuccessMessage"] = $"Registration submitted for '{model.ActivityTitle}'! Status is Pending approval by the community coordinator.";
            return RedirectToAction(nameof(Details), new { id = model.ActivityId });
        }

        [HttpGet]
        public async Task<IActionResult> MyActivities()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var registrations = await _activityService.GetUserRegistrationsAsync(user.Id);
            return View(registrations);
        }
    }
}
