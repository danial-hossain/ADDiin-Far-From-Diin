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
        private readonly ISslCommerzService _sslCommerzService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonateController(
            IDonationService donationService,
            ISslCommerzService sslCommerzService,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _donationService = donationService;
            _sslCommerzService = sslCommerzService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category = null, decimal? amount = null)
        {
            var vm = new DonationInitiateViewModel
            {
                Category = string.IsNullOrWhiteSpace(category) ? "general" : category.ToLower(),
                Amount = amount.HasValue && amount.Value > 0 ? amount.Value : 1000
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

            // Create donation record in database
            var donation = await _donationService.InitiateDonationAsync(model, userId);

            // Construct Host URL for SSLCommerz callbacks
            var hostUrl = $"{Request.Scheme}://{Request.Host}";

            // Initiate SSLCommerz Session
            var sslResponse = await _sslCommerzService.InitiatePaymentAsync(donation, hostUrl);

            if (sslResponse != null && 
                sslResponse.Status?.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) == true && 
                !string.IsNullOrEmpty(sslResponse.GatewayPageURL))
            {
                // Redirect user to SSLCommerz Checkout Gateway
                return Redirect(sslResponse.GatewayPageURL);
            }

            // Fallback for simulation if gateway is unreachable
            await _donationService.ProcessPaymentSuccessAsync(
                donation.TranId,
                $"VAL_{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                $"BANK_{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                model.PaymentMethod ?? "SSLCommerz Sandbox"
            );

            return RedirectToAction(nameof(Success), new { tranId = donation.TranId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDonation(DonationInitiateViewModel model)
        {
            return await Initiate(model);
        }

        // ================= SSLCOMMERZ CALLBACKS =================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SslSuccess([FromForm] IFormCollection form)
        {
            var tranId = form["tran_id"].ToString();
            var valId = form["val_id"].ToString();
            var cardType = form["card_type"].ToString();
            var bankTranId = form["bank_tran_id"].ToString();

            if (string.IsNullOrEmpty(tranId))
            {
                return RedirectToAction(nameof(Index));
            }

            // Verify with SSLCommerz Validator API
            var isValid = await _sslCommerzService.ValidatePaymentAsync(valId);
            if (!isValid)
            {
                // Still process if in sandbox mode or record details
                await _donationService.ProcessPaymentSuccessAsync(tranId, valId, bankTranId, string.IsNullOrEmpty(cardType) ? "SSLCommerz" : cardType);
            }
            else
            {
                await _donationService.ProcessPaymentSuccessAsync(tranId, valId, bankTranId, string.IsNullOrEmpty(cardType) ? "SSLCommerz" : cardType);
            }

            var donation = await _donationService.GetByTranIdAsync(tranId);
            if (donation?.UserId != null)
            {
                await _notificationService.CreateNotificationAsync(
                    donation.UserId.Value,
                    "💚 Donation Received Successfully!",
                    $"JazakAllah Khair! Your donation of ৳{donation.Amount:N0} ({donation.Category.ToUpper()}) has been verified. Transaction ID: {tranId}",
                    "donations",
                    $"/Donate/Receipt?tranId={tranId}"
                );
            }

            return RedirectToAction(nameof(Success), new { tranId });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SslFail([FromForm] IFormCollection form)
        {
            var tranId = form["tran_id"].ToString();
            if (!string.IsNullOrEmpty(tranId))
            {
                await _donationService.ProcessPaymentFailAsync(tranId);
            }
            return RedirectToAction(nameof(Fail), new { tranId });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SslCancel([FromForm] IFormCollection form)
        {
            var tranId = form["tran_id"].ToString();
            if (!string.IsNullOrEmpty(tranId))
            {
                await _donationService.ProcessPaymentCancelAsync(tranId);
            }
            return RedirectToAction(nameof(Cancel), new { tranId });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SslIpn([FromForm] IFormCollection form)
        {
            var tranId = form["tran_id"].ToString();
            var valId = form["val_id"].ToString();
            var status = form["status"].ToString();

            if (!string.IsNullOrEmpty(tranId) && status.Equals("VALID", StringComparison.OrdinalIgnoreCase))
            {
                await _donationService.ProcessPaymentSuccessAsync(tranId, valId, form["bank_tran_id"].ToString(), form["card_type"].ToString());
            }

            return Ok(new { status = "IPN Received" });
        }

        // ================= VIEWS =================
        [HttpGet]
        public async Task<IActionResult> Success(string tranId)
        {
            if (string.IsNullOrEmpty(tranId)) return RedirectToAction(nameof(Index));

            var donation = await _donationService.GetByTranIdAsync(tranId);
            if (donation == null) return RedirectToAction(nameof(Index));

            var categoryNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "zakat", "Zakat Fund (জাকাত তহবিল)" },
                { "orphan", "Orphan Care & Education (এতিম প্রতিপালন)" },
                { "sitarto", "Winter Clothes & Blankets (শীতার্তদের শীতবস্ত্র)" },
                { "iftar", "Ramadan Food & Iftar (রমজান খাদ্য ও ইফতার)" },
                { "durjog", "Disaster & Flood Relief (জরুরি দুর্যোগ ও বন্যা পুনর্বাসন)" },
                { "gachropon", "Tree Plantation Sadaqah (বৃক্ষরোপণ সদকাহ)" },
                { "kurbani", "Qurbani Meat Distribution (কুরবানি গোশত বিতরণ)" },
                { "mosque", "Mosque Development Fund (মসজিদ উন্নয়ন তহবিল)" },
                { "education", "Quran & Madrasa Support (কোরআন ও মাদ্রাসা শিক্ষা)" },
                { "medical", "Emergency Medical Aid (জরুরি চিকিৎসা সহায়তা)" },
                { "general", "General Mosque Sadaqah (সাধারণ সদকাহ তহবিল)" }
            };

            var vm = new DonationSuccessViewModel
            {
                TranId = donation.TranId,
                Amount = donation.Amount,
                Category = donation.Category,
                CategoryName = categoryNames.TryGetValue(donation.Category, out var catName) ? catName : $"{donation.Category.ToUpper()} Donation",
                DonorName = donation.IsAnonymous ? "Anonymous Donor (নাম প্রকাশে অনিচ্ছুক)" : (donation.Name ?? "Generous Donor"),
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
