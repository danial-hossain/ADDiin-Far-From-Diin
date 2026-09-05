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

    public class AIApiAskRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class AIApiAskResponse
    {
        public string Answer { get; set; } = string.Empty;
        public List<DiinAISource> Sources { get; set; } = new();
    }

    public class ProductTextAnalysisRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class HalalDetectorResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // HARAM_DETECTED, MUSHBOOH_DETECTED, NO_HARAM_MATCH, INSUFFICIENT_OCR, error

        [System.Text.Json.Serialization.JsonPropertyName("ocr")]
        public AdDiinAiOcr? Ocr { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("decision")]
        public AdDiinAiDecision? Decision { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("api")]
        public AdDiinAiApiInfo? Api { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public string? Error { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; }

        // Backward compatibility / convenience properties
        [System.Text.Json.Serialization.JsonPropertyName("prediction")]
        public string? Prediction => Status;

        [System.Text.Json.Serialization.JsonPropertyName("confidence")]
        public double Confidence => Ocr?.Confidence ?? 0.0;

        [System.Text.Json.Serialization.JsonPropertyName("rawOcr")]
        public string? RawOcr => Ocr?.Text;

        [System.Text.Json.Serialization.JsonPropertyName("reason")]
        public string? Reason => Decision?.Reason ?? Explanation;

        [System.Text.Json.Serialization.JsonPropertyName("ingredientsDetected")]
        public List<DetectedIngredient>? IngredientsDetected
        {
            get
            {
                var list = new List<DetectedIngredient>();
                if (Decision?.HaramEvidence != null)
                {
                    foreach (var h in Decision.HaramEvidence)
                    {
                        list.Add(new DetectedIngredient
                        {
                            Name = h.Ingredient,
                            Status = "Haram",
                            Reason = !string.IsNullOrWhiteSpace(h.Reference) ? $"{h.Description} ({h.Reference})" : h.Description,
                            MatchType = h.MatchType,
                            OcrIngredient = h.OcrIngredient,
                            Reference = h.Reference
                        });
                    }
                }
                if (Decision?.MushboohEvidence != null)
                {
                    foreach (var m in Decision.MushboohEvidence)
                    {
                        list.Add(new DetectedIngredient
                        {
                            Name = m.Ingredient,
                            Status = "Requires Verification",
                            Reason = !string.IsNullOrWhiteSpace(m.Reference) ? $"{m.Description} ({m.Reference})" : m.Description,
                            MatchType = m.MatchType,
                            OcrIngredient = m.OcrIngredient,
                            Reference = m.Reference
                        });
                    }
                }
                return list.Count > 0 ? list : null;
            }
        }
    }

    public class AdDiinAiOcr
    {
        [System.Text.Json.Serialization.JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    public class AdDiinAiDecision
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("haram_evidence")]
        public List<AdDiinAiEvidence> HaramEvidence { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("mushbooh_evidence")]
        public List<AdDiinAiEvidence> MushboohEvidence { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("semantic_candidates")]
        public List<AdDiinAiEvidence> SemanticCandidates { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("halal_certification")]
        public bool HalalCertification { get; set; } = false;
    }

    public class AdDiinAiEvidence
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string? Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ingredient")]
        public string Ingredient { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("match_type")]
        public string MatchType { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("ocr_ingredient")]
        public string? OcrIngredient { get; set; }
    }

    public class AdDiinAiApiInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("model")]
        public string? Model { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ocr_engine")]
        public string? OcrEngine { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("retrieval")]
        public string? Retrieval { get; set; }
    }

    public class DetectedIngredient
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // Halal, Haram, Requires Verification

        [System.Text.Json.Serialization.JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("matchType")]
        public string? MatchType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ocrIngredient")]
        public string? OcrIngredient { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }
}
