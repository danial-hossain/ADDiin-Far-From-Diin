using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdDiin.Models.Entities
{
    public class Donation
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "general"; // zakat, iftar, durjog, sitarto, gachropon, kurbani, orphan, general

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "BDT";

        [Required]
        [MaxLength(100)]
        public string TranId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ValId { get; set; }

        [MaxLength(100)]
        public string? BankTranId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "pending"; // pending, completed, failed, cancelled

        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // bkash, nagad, rocket, card, bank, sslcommerz

        public string? SslResponse { get; set; }

        public string? Notes { get; set; }

        public bool IsAnonymous { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
