using AdDiin.Models.Entities;

namespace AdDiin.Models.ViewModels
{
    public class MyDeenHubViewModel
    {
        public UserDeenSettings Settings { get; set; } = new();
        public DailyDeenGoal TodayGoals { get; set; } = new();
        public QuranReadingLog TodayQuran { get; set; } = new();
        public List<DhikrRecord> TodayDhikrRecords { get; set; } = new();
        public List<DhikrPresetItem> DhikrPresets { get; set; } = new();
        public List<AdhkarItemViewModel> MorningAdhkar { get; set; } = new();
        public List<AdhkarItemViewModel> EveningAdhkar { get; set; } = new();
        public List<RuqyahItemViewModel> RuqyahRoutines { get; set; } = new();
        public List<DailyProgressSummary> WeeklyProgress { get; set; } = new();
        
        public int TotalDhikrCountToday { get; set; }
        public int DailyProgressPercent { get; set; }
        public int WeeklyAveragePercent { get; set; }
        public int MonthlyProgressPercent { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int OverallGoalCompletionRate { get; set; }
    }

    public class DhikrPresetItem
    {
        public string Name { get; set; } = string.Empty;
        public string Arabic { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public int DefaultTarget { get; set; } = 100;
        public int CurrentCount { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
    }

    public class AdhkarItemViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Arabic { get; set; } = string.Empty;
        public string Transliteration { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string Benefit { get; set; } = string.Empty;
        public int TargetCount { get; set; } = 1;
        public bool IsCompleted { get; set; }
    }

    public class RuqyahItemViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Arabic { get; set; } = string.Empty;
        public string Transliteration { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string Benefit { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public int RecitationCount { get; set; } = 3;
        public bool IsCompleted { get; set; }
    }

    public class DailyProgressSummary
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public int PrayersDone { get; set; }
        public bool QuranDone { get; set; }
        public bool AdhkarDone { get; set; }
        public bool DhikrDone { get; set; }
        public bool RuqyahDone { get; set; }
    }

    public class NotificationsPageViewModel
    {
        public List<UserNotification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public string SelectedCategory { get; set; } = "all";
        public UserDeenSettings Settings { get; set; } = new();
    }

    public class ProgramRegistrationInputModel
    {
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class IslamicCalendarPageViewModel
    {
        public string CurrentHijriDateString { get; set; } = string.Empty;
        public string CurrentGregorianDateString { get; set; } = string.Empty;
        public string CurrentHijriMonthName { get; set; } = string.Empty;
        public int CurrentHijriYear { get; set; } = 1447;
        public List<IslamicEvent> UpcomingEvents { get; set; } = new();
        public List<HijriCalendarDay> CalendarDays { get; set; } = new();
    }

    public class HijriCalendarDay
    {
        public int DayNumber { get; set; }
        public int HijriDay { get; set; }
        public string HijriMonth { get; set; } = string.Empty;
        public bool IsToday { get; set; }
        public bool HasOccasion { get; set; }
        public string? OccasionTitle { get; set; }
        public bool IsWhiteDay { get; set; } // 13, 14, 15 fasting
        public bool IsSunnahFasting { get; set; } // Mon / Thu
    }

    public class UserProfileDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public ProfileViewModel ProfileForm { get; set; } = new();
        
        // Progress Metrics
        public int PrayerCompletionRate { get; set; }
        public int QuranProgressPages { get; set; }
        public int DhikrTotalCount { get; set; }
        public int AdhkarCompletionRate { get; set; }
        public int RuqyahCompletionRate { get; set; }
        public int OverallCompletionRate { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }

        // Activities
        public List<ProgramRegistration> RegisteredPrograms { get; set; } = new();
        public List<Activity> UpcomingPrograms { get; set; } = new();
        public List<ProgramRegistration> CompletedPrograms { get; set; } = new();
        public int ActivitiesJoinedCount { get; set; }

        // Charity
        public List<Donation> DonationHistory { get; set; } = new();
        public decimal MonthlyDonationGoal { get; set; }
        public decimal TotalDonatedThisMonth { get; set; }
        public int CharityGoalProgressPercent { get; set; }
        public decimal LifetimeDonated { get; set; }

        // Achievements
        public List<UserAchievementBadge> Badges { get; set; } = new();
    }

    public class UserAchievementBadge
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public string ProgressText { get; set; } = string.Empty;
        public DateTime? UnlockedAt { get; set; }
    }
}
