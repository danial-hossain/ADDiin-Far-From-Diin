using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdDiin.Models.Entities
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(255)]
        public string? ImagePublicId { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = "Islamic Seminar"; // Waz Mahfil, Islamic Seminar, Quran Competition, Islamic Workshop, Islamic Gathering, Community Islamic Programs

        [MaxLength(255)]
        public string? Location { get; set; } = "Central Islamic Center & Online";

        [MaxLength(255)]
        public string? Organizer { get; set; } = "ADDiin Community";

        [MaxLength(255)]
        public string? Instructor { get; set; }

        public DateTime? ProgramDate { get; set; } = DateTime.Today.AddDays(7);

        [MaxLength(50)]
        public string? StartTime { get; set; } = "05:00 PM";

        [MaxLength(50)]
        public string? EndTime { get; set; } = "07:00 PM";

        public int? MaxCapacity { get; set; } = 150;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation collections
        public virtual ICollection<ProgramRegistration> Registrations { get; set; } = new List<ProgramRegistration>();
    }

    public class ProgramRegistration
    {
        public int Id { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [ForeignKey("ActivityId")]
        public virtual Activity? Activity { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [MaxLength(500)]
        public string? AdminRemarks { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }
    }
}
