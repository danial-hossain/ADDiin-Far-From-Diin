using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdDiin.Models.Entities
{
    public class DhikrRecord
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [MaxLength(100)]
        public string DhikrName { get; set; } = "SubhanAllah";

        public int Count { get; set; }

        public int TargetCount { get; set; } = 100;

        public DateTime Date { get; set; } = DateTime.Today;

        public bool IsTargetAchieved { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class QuranReadingLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [MaxLength(30)]
        public string GoalType { get; set; } = "pages"; // pages, verses, minutes

        public int DailyTarget { get; set; } = 10;

        [MaxLength(100)]
        public string CurrentSurah { get; set; } = "Al-Fatihah";

        public int CurrentAyah { get; set; } = 1;

        public int PagesReadToday { get; set; } = 0;

        public int VersesReadToday { get; set; } = 0;

        public DateTime Date { get; set; } = DateTime.Today;

        public bool IsCompleted { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AdhkarLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [MaxLength(20)]
        public string AdhkarType { get; set; } = "Morning"; // Morning, Evening

        [Required]
        [MaxLength(100)]
        public string ItemKey { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Today;

        public bool IsCompleted { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    public class RuqyahLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoutineType { get; set; } = "Daily Protection"; // Daily Protection, Morning Routine, Evening Routine, Self Recitation

        [Required]
        [MaxLength(100)]
        public string ItemKey { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Today;

        public bool IsCompleted { get; set; }

        public bool ReminderEnabled { get; set; } = true;

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    public class DailyDeenGoal
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public bool Fajr { get; set; }
        public bool Dhuhr { get; set; }
        public bool Asr { get; set; }
        public bool Maghrib { get; set; }
        public bool Isha { get; set; }

        public bool QuranRead { get; set; }
        public bool MorningAdhkar { get; set; }
        public bool EveningAdhkar { get; set; }
        public bool DhikrTarget { get; set; }
        public bool RuqyahRoutine { get; set; }
        public bool CharityGiven { get; set; }

        public int CompletionPercentage { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserNotification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = "general"; // prayer, mydeen, activities, calendar, general

        [MaxLength(255)]
        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserDeenSettings
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public int DailyDhikrTarget { get; set; } = 100;

        public int DailyQuranPagesTarget { get; set; } = 5;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyDonationGoal { get; set; } = 2000;

        public bool PrayerReminder { get; set; } = true;
        public bool QuranReminder { get; set; } = true;
        public bool DhikrReminder { get; set; } = true;
        public bool AdhkarReminder { get; set; } = true;
        public bool RuqyahReminder { get; set; } = true;
        public bool ProgramReminder { get; set; } = true;
        public bool CalendarReminder { get; set; } = true;

        public int CurrentStreak { get; set; } = 1;
        public int LongestStreak { get; set; } = 1;
        public DateTime? LastActiveDate { get; set; } = DateTime.Today;
    }
}
