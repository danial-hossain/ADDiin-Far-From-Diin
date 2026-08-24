using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    public class IslamicEvent
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(100)]
        public string? HijriDate { get; set; }

        [MaxLength(50)]
        public string? HijriMonth { get; set; }

        public int? HijriDay { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = "religious"; // special, religious, festival, historical

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
