using AdDiin.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            // Ensure database is created and migrations applied
            try
            {
                await context.Database.MigrateAsync();
            }
            catch
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 1. Seed Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@addiin.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "ADDiin Community Coordinator",
                    PhoneNumber = "+8801700000000",
                    Address = "ADDiin Center, Dhaka",
                    City = "Dhaka",
                    PostalCode = "1205",
                    Gender = "Male",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Regular Demo User
            var testEmail = "test@test.com";
            var testUser = await userManager.FindByEmailAsync(testEmail);
            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    FullName = "Danial Hossain Dani",
                    PhoneNumber = "+8801811111111",
                    Address = "Tejgaon Industrial Area",
                    City = "Dhaka",
                    PostalCode = "1208",
                    Gender = "Male",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(testUser, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "User");
                }
            }

            // 4. Seed Prayer Times (Personal Daily Prayer Schedule)
            if (!await context.PrayerTimes.AnyAsync())
            {
                var prayerTimes = new List<PrayerTime>
                {
                    // Daily 5 Obligatory Prayers
                    new() { PrayerName = "fajr_azan", PrayerTimeValue = new TimeSpan(5, 0, 0), DisplayNameEn = "Fajr (Dawn)", DisplayNameBn = "ফজর", Category = "fard", PrayerType = "fard", DisplayOrder = 1, IsActive = true },
                    new() { PrayerName = "dhuhr_azan", PrayerTimeValue = new TimeSpan(12, 15, 0), DisplayNameEn = "Dhuhr (Noon)", DisplayNameBn = "যোহর", Category = "fard", PrayerType = "fard", DisplayOrder = 3, IsActive = true },
                    new() { PrayerName = "asr_azan", PrayerTimeValue = new TimeSpan(15, 45, 0), DisplayNameEn = "Asr (Afternoon)", DisplayNameBn = "আসর", Category = "fard", PrayerType = "fard", DisplayOrder = 5, IsActive = true },
                    new() { PrayerName = "maghrib_azan", PrayerTimeValue = new TimeSpan(18, 30, 0), DisplayNameEn = "Maghrib (Sunset)", DisplayNameBn = "মাগরিব", Category = "fard", PrayerType = "fard", DisplayOrder = 7, IsActive = true },
                    new() { PrayerName = "isha_azan", PrayerTimeValue = new TimeSpan(20, 0, 0), DisplayNameEn = "Isha (Night)", DisplayNameBn = "ইশা", Category = "fard", PrayerType = "fard", DisplayOrder = 9, IsActive = true },

                    // Recommended Sunnah & Voluntary Nafl Prayers
                    new() { PrayerName = "tahajjut", PrayerTimeValue = new TimeSpan(2, 30, 0), DisplayNameEn = "Tahajjud (Night Vigil)", DisplayNameBn = "তাহাজ্জুদ", Category = "nafl", PrayerType = "optional", DisplayOrder = 11, IsActive = true },
                    new() { PrayerName = "ishraq", PrayerTimeValue = new TimeSpan(5, 45, 0), DisplayNameEn = "Ishraq (Post-Sunrise)", DisplayNameBn = "ইশরাক", Category = "nafl", PrayerType = "optional", DisplayOrder = 12, IsActive = true },
                    new() { PrayerName = "duha", PrayerTimeValue = new TimeSpan(8, 0, 0), DisplayNameEn = "Duha / Chasht (Forenoon)", DisplayNameBn = "দুহা (চাশত)", Category = "nafl", PrayerType = "optional", DisplayOrder = 13, IsActive = true },
                    new() { PrayerName = "awwabin", PrayerTimeValue = new TimeSpan(18, 45, 0), DisplayNameEn = "Awwabin (Post-Maghrib)", DisplayNameBn = "আওয়াবীন", Category = "nafl", PrayerType = "optional", DisplayOrder = 14, IsActive = true }
                };

                await context.PrayerTimes.AddRangeAsync(prayerTimes);
                await context.SaveChangesAsync();
            }

            // 5. Seed Islamic Calendar Events
            if (!await context.IslamicEvents.AnyAsync())
            {
                var events = new List<IslamicEvent>
                {
                    new() { EventName = "Ramadan 2026", EventDate = DateTime.Today.AddDays(10), HijriDate = "1 Ramadan 1447 AH", HijriMonth = "Ramadan", HijriDay = 1, EventType = "religious", Description = "First day of the holy month of Ramadan: spiritual fasting, nightly Taraweeh, and intensified Quran recitation.", DisplayOrder = 1, IsActive = true },
                    new() { EventName = "Laylat al-Qadr 2026", EventDate = DateTime.Today.AddDays(36), HijriDate = "27 Ramadan 1447 AH", HijriMonth = "Ramadan", HijriDay = 27, EventType = "special", Description = "The Night of Power, better than a thousand months. Dedicated to Qiyam-ul-Layl and earnest dua.", DisplayOrder = 2, IsActive = true },
                    new() { EventName = "Eid ul-Fitr 2026", EventDate = DateTime.Today.AddDays(40), HijriDate = "1 Shawwal 1447 AH", HijriMonth = "Shawwal", HijriDay = 1, EventType = "festival", Description = "Festival of Gratitude & Breaking the Fast with morning prayers, takbeerat, and community joy.", DisplayOrder = 3, IsActive = true },
                    new() { EventName = "Day of Arafah & Hajj 2026", EventDate = DateTime.Today.AddDays(105), HijriDate = "9 Dhul Hijjah 1447 AH", HijriMonth = "Dhul Hijjah", HijriDay = 9, EventType = "religious", Description = "Day of Arafah, the pinnacle pillar of Hajj and recommended Sunnah fasting day for all believers.", DisplayOrder = 4, IsActive = true },
                    new() { EventName = "Eid al-Adha 2026", EventDate = DateTime.Today.AddDays(106), HijriDate = "10 Dhul Hijjah 1447 AH", HijriMonth = "Dhul Hijjah", HijriDay = 10, EventType = "festival", Description = "Festival of Sacrifice (Qurbani) commemorating the exemplary devotion of Prophet Ibrahim (AS).", DisplayOrder = 5, IsActive = true },
                    new() { EventName = "Islamic New Year (1448 AH)", EventDate = DateTime.Today.AddDays(125), HijriDate = "1 Muharram 1448 AH", HijriMonth = "Muharram", HijriDay = 1, EventType = "religious", Description = "Commencement of the new Islamic Hijri Year 1448.", DisplayOrder = 6, IsActive = true },
                    new() { EventName = "Day of Ashura", EventDate = DateTime.Today.AddDays(134), HijriDate = "10 Muharram 1448 AH", HijriMonth = "Muharram", HijriDay = 10, EventType = "historical", Description = "Day of Ashura commemorating historical milestones and Sunnah fasting (9th & 10th Muharram).", DisplayOrder = 7, IsActive = true },
                    new() { EventName = "Mawlid / Seerat Conference", EventDate = DateTime.Today.AddDays(195), HijriDate = "12 Rabi ul-Awwal 1448 AH", HijriMonth = "Rabi ul-Awwal", HijriDay = 12, EventType = "festival", Description = "Reflecting on the life, character, and sublime legacy of Prophet Muhammad (pbuh).", DisplayOrder = 8, IsActive = true }
                };

                await context.IslamicEvents.AddRangeAsync(events);
                await context.SaveChangesAsync();
            }

            // 6. Seed Islamic Activities & Programs (User-centric program discovery)
            if (!await context.Activities.AnyAsync())
            {
                var activities = new List<Activity>
                {
                    new()
                    {
                        Title = "Annual Grand Waz Mahfil & Seerat Conference",
                        Description = "Inspiring spiritual discourses on building a Quran-centric life, purifying the heart (Tazkiyah), and following Prophetic character in the modern age.",
                        Category = "Waz Mahfil",
                        Location = "Grand Auditorium, Dhaka & Live Stream",
                        Organizer = "ADDiin Islamic Youth Network",
                        Instructor = "Dr. Mizanur Rahman Azhari & Scholars",
                        ProgramDate = DateTime.Today.AddDays(5),
                        StartTime = "05:30 PM",
                        EndTime = "09:30 PM",
                        MaxCapacity = 500,
                        ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "National Youth Quran Recitation & Hifz Competition",
                        Description = "Interactive competition evaluating melodious Tajweed, accurate memorization, and understanding of selected Surahs with scholarly certificates and awards.",
                        Category = "Quran Competition",
                        Location = "Islamic Cultural Center, Dhanmondi",
                        Organizer = "ADDiin Quran Academy",
                        Instructor = "Qari Ahmadullah & Panel of Huffaz",
                        ProgramDate = DateTime.Today.AddDays(12),
                        StartTime = "09:00 AM",
                        EndTime = "04:00 PM",
                        MaxCapacity = 200,
                        ImageUrl = "https://images.unsplash.com/photo-1609599006353-e629aaabfeae?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "Islamic Finance & Halal Investment Seminar",
                        Description = "Practical workshop on Islamic principles of wealth, modern banking alternatives, crypto fiqh evaluation, and calculating personal business Zakat accurately.",
                        Category = "Islamic Seminar",
                        Location = "Virtual Zoom Live & Campus Hall",
                        Organizer = "ADDiin Center for Islamic Economics",
                        Instructor = "Mufti Zubair Ahmad (Islamic Finance Specialist)",
                        ProgramDate = DateTime.Today.AddDays(18),
                        StartTime = "07:00 PM",
                        EndTime = "09:00 PM",
                        MaxCapacity = 300,
                        ImageUrl = "https://images.unsplash.com/photo-1593113598332-cd288d649433?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 3,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "Tadabbur-ul-Quran & Arabic Essentials Workshop",
                        Description = "Hands-on intensive workshop designed for students and professionals to unlock the vocabulary, grammar patterns, and deep reflection tools of the Holy Quran.",
                        Category = "Islamic Workshop",
                        Location = "Online Interactive Cohort",
                        Organizer = "ADDiin Learning Circle",
                        Instructor = "Ustadh Salman Al-Farisi",
                        ProgramDate = DateTime.Today.AddDays(22),
                        StartTime = "08:00 PM",
                        EndTime = "10:00 PM",
                        MaxCapacity = 150,
                        ImageUrl = "https://images.unsplash.com/photo-1507842229451-7f01be8510d2?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 4,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "Monthly Community Family Halaqah & Dua Gathering",
                        Description = "Spiritual gathering for families and young seekers featuring discussions on raising pious children, overcoming modern distractions, and collective supplication.",
                        Category = "Islamic Gathering",
                        Location = "Community Center Hall & Online",
                        Organizer = "ADDiin Family Fellowship",
                        Instructor = "Shaykh Abdur Rahman",
                        ProgramDate = DateTime.Today.AddDays(28),
                        StartTime = "06:00 PM",
                        EndTime = "08:30 PM",
                        MaxCapacity = 250,
                        ImageUrl = "https://images.unsplash.com/photo-1542601906990-b4d3fb778b09?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 5,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "Community Winter Clothes & Warmth Drive",
                        Description = "Volunteer-led distribution campaign packaging warm blankets, winter garments, and essentials for underprivileged families across northern regions.",
                        Category = "Community Islamic Programs",
                        Location = "ADDiin Volunteer Hub, Dhaka",
                        Organizer = "ADDiin Humanitarian Wing",
                        Instructor = "Volunteer Coordination Team",
                        ProgramDate = DateTime.Today.AddDays(15),
                        StartTime = "10:00 AM",
                        EndTime = "05:00 PM",
                        MaxCapacity = 100,
                        ImageUrl = "https://images.unsplash.com/photo-1469571486292-0ba58a3f068b?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 6,
                        IsActive = true
                    }
                };

                await context.Activities.AddRangeAsync(activities);
                await context.SaveChangesAsync();
            }

            // 7. Seed Sample Program Registrations
            if (!await context.ProgramRegistrations.AnyAsync() && testUser != null)
            {
                var firstActivity = await context.Activities.FirstOrDefaultAsync();
                if (firstActivity != null)
                {
                    var registration = new ProgramRegistration
                    {
                        ActivityId = firstActivity.Id,
                        UserId = testUser.Id,
                        FullName = testUser.FullName,
                        Email = testUser.Email ?? "test@test.com",
                        PhoneNumber = testUser.PhoneNumber ?? "+8801811111111",
                        Notes = "Attending with family members. Interested in Seerat study materials.",
                        Status = "Approved",
                        AdminRemarks = "Approved! Welcome to the conference.",
                        RegisteredAt = DateTime.UtcNow.AddDays(-2),
                        ReviewedAt = DateTime.UtcNow.AddDays(-1)
                    };
                    context.ProgramRegistrations.Add(registration);
                    await context.SaveChangesAsync();
                }
            }

            // 8. Seed Verified Sample Donations
            if (!await context.Donations.AnyAsync() && testUser != null)
            {
                var sampleDonations = new List<Donation>
                {
                    new()
                    {
                        UserId = testUser.Id,
                        Name = testUser.FullName,
                        Email = testUser.Email,
                        Phone = testUser.PhoneNumber,
                        Category = "zakat",
                        Amount = 15000,
                        Currency = "BDT",
                        TranId = "DON_1712000001_ZAKAT",
                        PaymentStatus = "completed",
                        PaymentMethod = "bkash",
                        ValId = "VAL_BKASH_9921",
                        BankTranId = "TXN_7829104",
                        IsAnonymous = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-12)
                    },
                    new()
                    {
                        UserId = testUser.Id,
                        Name = testUser.FullName,
                        Email = testUser.Email,
                        Phone = testUser.PhoneNumber,
                        Category = "iftar",
                        Amount = 5000,
                        Currency = "BDT",
                        TranId = "DON_1712000002_IFTAR",
                        PaymentStatus = "completed",
                        PaymentMethod = "nagad",
                        ValId = "VAL_NAGAD_4421",
                        BankTranId = "TXN_8812933",
                        IsAnonymous = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new()
                    {
                        UserId = null,
                        Name = "Generous Well-wisher",
                        Email = "anonymous@donor.org",
                        Phone = "+8801999999999",
                        Category = "orphan",
                        Amount = 10000,
                        Currency = "BDT",
                        TranId = "DON_1712000003_ORPHAN",
                        PaymentStatus = "completed",
                        PaymentMethod = "card",
                        ValId = "VAL_CARD_1102",
                        BankTranId = "TXN_3391024",
                        IsAnonymous = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                };

                await context.Donations.AddRangeAsync(sampleDonations);
                await context.SaveChangesAsync();
            }

            // 9. Seed Demo User My Deen Stats
            if (testUser != null)
            {
                var hasSettings = await context.UserDeenSettings.AnyAsync(s => s.UserId == testUser.Id);
                if (!hasSettings)
                {
                    var deenSettings = new UserDeenSettings
                    {
                        UserId = testUser.Id,
                        DailyDhikrTarget = 100,
                        DailyQuranPagesTarget = 10,
                        MonthlyDonationGoal = 5000,
                        PrayerReminder = true,
                        QuranReminder = true,
                        DhikrReminder = true,
                        AdhkarReminder = true,
                        RuqyahReminder = true,
                        ProgramReminder = true,
                        CalendarReminder = true,
                        CurrentStreak = 8,
                        LongestStreak = 14,
                        LastActiveDate = DateTime.Today
                    };
                    context.UserDeenSettings.Add(deenSettings);
                }

                var today = DateTime.Today;
                var hasTodayGoal = await context.DailyDeenGoals.AnyAsync(g => g.UserId == testUser.Id && g.Date == today);
                if (!hasTodayGoal)
                {
                    var dailyGoal = new DailyDeenGoal
                    {
                        UserId = testUser.Id,
                        Date = today,
                        Fajr = true,
                        Dhuhr = true,
                        Asr = true,
                        Maghrib = false,
                        Isha = false,
                        QuranRead = true,
                        MorningAdhkar = true,
                        EveningAdhkar = false,
                        DhikrTarget = true,
                        RuqyahRoutine = true,
                        CharityGiven = true,
                        CompletionPercentage = 80,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.DailyDeenGoals.Add(dailyGoal);
                }

                // Dhikr Records
                var hasDhikr = await context.DhikrRecords.AnyAsync(d => d.UserId == testUser.Id && d.Date == today);
                if (!hasDhikr)
                {
                    context.DhikrRecords.AddRange(new List<DhikrRecord>
                    {
                        new() { UserId = testUser.Id, DhikrName = "SubhanAllah", Count = 100, TargetCount = 100, Date = today, IsTargetAchieved = true },
                        new() { UserId = testUser.Id, DhikrName = "Alhamdulillah", Count = 100, TargetCount = 100, Date = today, IsTargetAchieved = true },
                        new() { UserId = testUser.Id, DhikrName = "Allahu Akbar", Count = 100, TargetCount = 100, Date = today, IsTargetAchieved = true },
                        new() { UserId = testUser.Id, DhikrName = "Astaghfirullah", Count = 70, TargetCount = 100, Date = today, IsTargetAchieved = false }
                    });
                }

                // Quran Log
                var hasQuran = await context.QuranReadingLogs.AnyAsync(q => q.UserId == testUser.Id && q.Date == today);
                if (!hasQuran)
                {
                    context.QuranReadingLogs.Add(new QuranReadingLog
                    {
                        UserId = testUser.Id,
                        GoalType = "pages",
                        DailyTarget = 10,
                        CurrentSurah = "Surah Al-Kahf",
                        CurrentAyah = 28,
                        PagesReadToday = 10,
                        Date = today,
                        IsCompleted = true,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Notifications
                var hasNotif = await context.UserNotifications.AnyAsync(n => n.UserId == testUser.Id);
                if (!hasNotif)
                {
                    context.UserNotifications.AddRange(new List<UserNotification>
                    {
                        new()
                        {
                            UserId = testUser.Id,
                            Title = "🔥 7-Day Consistency Streak Achieved!",
                            Message = "MashaAllah! You have completed your daily Islamic goals for 7 consecutive days. Keep going!",
                            Category = "mydeen",
                            LinkUrl = "/user-profile",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow.AddHours(-1)
                        },
                        new()
                        {
                            UserId = testUser.Id,
                            Title = "✅ Registration Approved",
                            Message = "Your registration for 'Annual Grand Waz Mahfil & Seerat Conference' has been approved.",
                            Category = "activities",
                            LinkUrl = "/user-profile",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow.AddHours(-3)
                        },
                        new()
                        {
                            UserId = testUser.Id,
                            Title = "🕌 Dhuhr Prayer Time Alert",
                            Message = "Dhuhr prayer time is active. Take a moment to connect with Allah.",
                            Category = "prayer",
                            LinkUrl = "/prayer-times",
                            IsRead = true,
                            CreatedAt = DateTime.UtcNow.AddHours(-5)
                        }
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
