using AdDiin.Models.Entities;

namespace AdDiin.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminCount { get; set; }
        public int RegularUserCount { get; set; }
        public decimal TotalDonationsCompleted { get; set; }
        public int TotalDonationsCount { get; set; }
        public int PendingDonationsCount { get; set; }
        public int PendingRegistrationsCount { get; set; }
        public int TotalRegistrationsCount { get; set; }
        public int PendingMiladCount { get; set; }
        public int TotalMiladCount { get; set; }
        public int TotalEventsCount { get; set; }
        public int TotalActivitiesCount { get; set; }
        public int UnreadContactCount { get; set; }
        public int ActiveConversationsCount { get; set; }

        public List<Donation> RecentDonations { get; set; } = new();
        public List<ProgramRegistration> RecentRegistrations { get; set; } = new();
        public List<MiladRequest> RecentMiladRequests { get; set; } = new();
        public List<IslamicEvent> UpcomingEvents { get; set; } = new();
        public List<Activity> ActivePrograms { get; set; } = new();
        public Dictionary<string, decimal> DonationsByCategory { get; set; } = new();
    }

    public class AdminDonationsViewModel
    {
        public List<Donation> Donations { get; set; } = new();
        public decimal TotalCompletedAmount { get; set; }
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SearchQuery { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
    }

    public class AdminUsersViewModel
    {
        public List<ApplicationUser> Users { get; set; } = new();
        public Dictionary<int, IList<string>> UserRoles { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int AdminCount { get; set; }
    }

    public class AdminRegistrationsViewModel
    {
        public List<ProgramRegistration> Registrations { get; set; } = new();
        public string? StatusFilter { get; set; }
        public string? SearchQuery { get; set; }
        public int? ActivityFilter { get; set; }
        public List<Activity> AvailableActivities { get; set; } = new();
    }

    public class AdminMiladsViewModel
    {
        public List<MiladRequest> MiladRequests { get; set; } = new();
        public string? StatusFilter { get; set; }
        public string? SearchQuery { get; set; }
    }
}
