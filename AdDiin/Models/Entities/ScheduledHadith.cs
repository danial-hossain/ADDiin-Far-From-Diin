using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    public class ScheduledHadith
    {
        public int Id { get; set; }

        public DateTime SlotDate { get; set; }

        public TimeSpan SlotTime { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Source { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
