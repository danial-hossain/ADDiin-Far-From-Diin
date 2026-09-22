# Route map

The application uses conventional MVC routing plus user-facing aliases
registered in `Program.cs`. Controllers remain responsible for actions while
aliases keep public URLs readable.

| Feature | Public route | Controller/action |
| --- | --- | --- |
| Home | `/` | `Home/Index` |
| About | `/about` | `Home/About` |
| Prayer times | `/prayer-times` | `PrayerTimes/Index` |
| Islamic calendar | `/islamic-calendar` | `IslamicCalendar/Index` |
| Activities | `/activities` | `Activities/Index` |
| Activity details | `/activities/{id}` | `Activities/Details` |
| My activities | `/my-activities` | `Activities/MyActivities` |
| Zakat | `/zakat` | `Zakat/Index` |
| Donations | `/donate` | `Donate/Index` |
| My donations | `/my-donations` | `Donate/MyDonations` |
| Messaging | `/messaging` | `Messages/Index` |
| Diin AI | `/diin-ai` | `DiinAI/Index` |
| Product analyzer | `/product-analyzer` | `ProductAnalyzer/Index` |
| Login | `/user-login` | `Account/Login` |
| Registration | `/user-registration` | `Account/Register` |
| Profile | `/user-profile` | `Account/Profile` |
| Admin dashboard | `/admin-dashboard` | `Admin/Dashboard` |

State-changing actions should continue to use antiforgery protection unless
they are provider callbacks that cannot send the browser form token. Authorization
is enforced by the relevant controller or action for user and administrator
areas.
