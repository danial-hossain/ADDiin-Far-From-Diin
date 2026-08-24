using AdDiin.Models.Entities;

namespace AdDiin.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<PrayerTime> JamaatPrayers { get; set; } = new();
        public List<PrayerTime> AzanPrayers { get; set; } = new();
        public List<IslamicEvent> UpcomingEvents { get; set; } = new();
        public List<Activity> OngoingActivities { get; set; } = new();
        public PrayerTime? NextPrayer { get; set; }
        public TimeSpan TimeUntilNextPrayer { get; set; }
    }
}
