using System.ComponentModel.DataAnnotations;

namespace AdDiin.Models.Entities
{
    public class PrayerTime
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string PrayerName { get; set; } = string.Empty; // e.g. fajr_azan, fajr_jamaat

        [Required]
        public TimeSpan PrayerTimeValue { get; set; }

        [Required]
        [MaxLength(50)]
        public string DisplayNameEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DisplayNameBn { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Category { get; set; } = "fard"; // fard, nafl, wajib, sunnah

        [Required]
        [MaxLength(20)]
        public string PrayerType { get; set; } = "azan"; // azan, jamaat, optional

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
