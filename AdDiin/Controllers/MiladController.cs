using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    [Authorize]
    public class MiladController : Controller
    {
        private readonly IMiladService _miladService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MiladController(IMiladService miladService, UserManager<ApplicationUser> userManager)
        {
            _miladService = miladService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var vm = new MiladCreateViewModel
            {
                Name = user?.FullName ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                MiladDate = DateTime.Today.AddDays(3)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MiladCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _miladService.CreateAsync(model, user.Id);
            TempData["SuccessMessage"] = "Your Milad / Dua Mehfil booking request has been submitted. The administration will review and confirm shortly.";

            return RedirectToAction(nameof(MyRequests));
        }

        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var requests = await _miladService.GetUserRequestsAsync(user.Id);
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _miladService.GetByIdAsync(id);
            if (request == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || (!User.IsInRole("Admin") && request.UserId != user.Id))
            {
                return Forbid();
            }

            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var request = await _miladService.GetByIdAsync(id);
            if (request == null || request.UserId != user.Id) return NotFound();

            if (request.Status != "pending")
            {
                TempData["ErrorMessage"] = "Only pending requests can be edited.";
                return RedirectToAction(nameof(MyRequests));
            }

            var vm = new MiladCreateViewModel
            {
                Name = request.Name,
                Phone = request.Phone,
                Description = request.Description,
                MiladDate = request.MiladDate
            };

            ViewBag.RequestId = id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MiladCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RequestId = id;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var updated = await _miladService.UpdateAsync(id, model, user.Id);
            if (updated == null)
            {
                TempData["ErrorMessage"] = "Unable to update the request.";
                return RedirectToAction(nameof(MyRequests));
            }

            TempData["SuccessMessage"] = "Milad request updated successfully.";
            return RedirectToAction(nameof(MyRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var success = await _miladService.CancelAsync(id, user.Id);
            if (success)
                TempData["SuccessMessage"] = "Milad request cancelled successfully.";
            else
                TempData["ErrorMessage"] = "Could not cancel request. Only pending requests can be cancelled.";

            return RedirectToAction(nameof(MyRequests));
        }
    }
}
