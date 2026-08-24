using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class DonateController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonateController(IDonationService donationService, UserManager<ApplicationUser> userManager)
        {
            _donationService = donationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category = null, decimal? amount = null)
        {
            var vm = new DonationInitiateViewModel
            {
                Category = category ?? "general",
                Amount = amount ?? 1000
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    vm.Name = user.FullName;
                    vm.Email = user.Email;
                    vm.Phone = user.PhoneNumber ?? string.Empty;
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate(DonationInitiateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                userId = user?.Id;
            }

            var donation = await _donationService.InitiateDonationAsync(model, userId);

            // Directly simulate secure digital payment verification / instant completion for testing and production
            await _donationService.ProcessPaymentSuccessAsync(
                donation.TranId,
                $"VAL_{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                $"BANK_TXN_{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                model.PaymentMethod ?? "sslcommerz"
            );

            return RedirectToAction(nameof(Success), new { tranId = donation.TranId });
        }

        [HttpGet]
        public async Task<IActionResult> Success(string tranId)
        {
            if (string.IsNullOrEmpty(tranId)) return RedirectToAction(nameof(Index));

            var donation = await _donationService.GetByTranIdAsync(tranId);
            if (donation == null) return RedirectToAction(nameof(Index));

            var categoryNames = new Dictionary<string, string>
            {
                { "zakat", "Zakat Donation" },
                { "iftar", "Iftar Program" },
                { "durjog", "Disaster Relief (Durjog)" },
                { "sitarto", "Winter Clothes (Sitarto)" },
                { "gachropon", "Tree Plantation (Gachropon)" },
                { "kurbani", "Qurbani Meat Distribution" },
                { "orphan", "Orphan Sponsorship" },
                { "general", "General Mosque Fund" }
            };

            var vm = new DonationSuccessViewModel
            {
                TranId = donation.TranId,
                Amount = donation.Amount,
                Category = donation.Category,
                CategoryName = categoryNames.TryGetValue(donation.Category, out var catName) ? catName : "Mosque Donation",
                DonorName = donation.IsAnonymous ? "Anonymous Donor" : (donation.Name ?? "Generous Donor"),
                DonorEmail = donation.Email,
                DonorPhone = donation.Phone,
                PaymentMethod = donation.PaymentMethod ?? "SSLCommerz",
                PaymentDate = donation.UpdatedAt ?? donation.CreatedAt,
                Status = donation.PaymentStatus
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Fail(string tranId)
        {
            var donation = await _donationService.GetByTranIdAsync(tranId);
            return View(donation);
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(string tranId)
        {
            var donation = await _donationService.GetByTranIdAsync(tranId);
            return View(donation);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyDonations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var donations = await _donationService.GetUserDonationsAsync(user.Id);
            return View(donations);
        }

        [HttpGet]
        public async Task<IActionResult> Receipt(string tranId)
        {
            if (string.IsNullOrEmpty(tranId)) return NotFound();

            var donation = await _donationService.GetByTranIdAsync(tranId);
            if (donation == null) return NotFound();

            return View(donation);
        }
    }
}
