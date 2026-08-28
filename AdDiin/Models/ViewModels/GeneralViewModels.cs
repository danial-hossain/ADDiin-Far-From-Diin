using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.ViewModels
{
    public class ZakatCalculatorViewModel
    {
        [Display(Name = "Cash in hand & Bank Accounts (৳)")]
        public decimal Cash { get; set; }

        [Display(Name = "Gold weight (grams)")]
        public decimal GoldWeight { get; set; }

        [Display(Name = "Gold current price per gram (৳)")]
        public decimal GoldPrice { get; set; } = 8500;

        [Display(Name = "Silver weight (grams)")]
        public decimal SilverWeight { get; set; }

        [Display(Name = "Silver current price per gram (৳)")]
        public decimal SilverPrice { get; set; } = 120;

        [Display(Name = "Investments & Shares (৳)")]
        public decimal Investments { get; set; }

        [Display(Name = "Business Inventory / Goods (৳)")]
        public decimal BusinessInventory { get; set; }

        [Display(Name = "Other Zakatable Assets (৳)")]
        public decimal OtherAssets { get; set; }

        [Display(Name = "Short-term Debts / Liabilities to Deduct (৳)")]
        public decimal Debts { get; set; }

        // Calculation outputs
        public decimal TotalAssets { get; set; }
        public decimal NetWealth { get; set; }
        public decimal NisabGoldThreshold { get; set; }
        public decimal NisabSilverThreshold { get; set; }
        public decimal NisabThresholdUsed { get; set; }
        public bool IsEligible { get; set; }
        public decimal ZakatPayable { get; set; }
        public bool HasCalculated { get; set; }
    }

    public class DonationInitiateViewModel
    {
        [Required(ErrorMessage = "Please select a donation category")]
        public string Category { get; set; } = "general"; // zakat, iftar, durjog, sitarto, gachropon, kurbani, orphan, general

        [Required(ErrorMessage = "Please enter an amount")]
        [Range(10, 10000000, ErrorMessage = "Donation amount must be between ৳10 and ৳10,000,000")]
        public decimal Amount { get; set; }

        [Display(Name = "Full Name")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Donate Anonymously")]
        public bool IsAnonymous { get; set; }

        public string? Notes { get; set; }

        public string? PaymentMethod { get; set; } = "sslcommerz"; // bkash, nagad, rocket, card, sslcommerz

        // Public live statistics
        public decimal TotalDonationsRaised { get; set; }
        public int TotalDonorsCount { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
    }

    public class DonationSuccessViewModel
    {
        public string TranId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? DonorName { get; set; }
        public string? DonorEmail { get; set; }
        public string? DonorPhone { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = "completed";
    }

    public class MiladCreateViewModel
    {
        [Required(ErrorMessage = "Event / Occasion title is required")]
        [MaxLength(255)]
        [Display(Name = "Event / Request Title")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone number is required")]
        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide details and purpose for the Milad/Dua")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Preferred date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Milad / Event Date")]
        public DateTime MiladDate { get; set; } = DateTime.Today.AddDays(2);
    }

    public class ContactViewModel
    {
        [Required(ErrorMessage = "Your name is required")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Your email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Company { get; set; }

        [Required(ErrorMessage = "Please enter your message")]
        public string Message { get; set; } = string.Empty;
    }

    public class DiinAIChatViewModel
    {
        public string UserMessage { get; set; } = string.Empty;
        public List<DiinAIChatMessage> History { get; set; } = new();
    }

    public class DiinAIChatMessage
    {
        public string Role { get; set; } = "user"; // user, assistant
        public string Content { get; set; } = string.Empty;
        public List<DiinAISource>? Sources { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DiinAISource
    {
        public string Source { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }
}
