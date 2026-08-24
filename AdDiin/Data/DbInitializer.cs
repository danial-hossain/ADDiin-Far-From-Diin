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
            await context.Database.MigrateAsync();

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
                    FullName = "Chief Mosque Administrator",
                    PhoneNumber = "+8801700000000",
                    Address = "Dhanmondi Central Mosque Complex",
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

            // 4. Seed Prayer Times
            if (!await context.PrayerTimes.AnyAsync())
            {
                var prayerTimes = new List<PrayerTime>
                {
                    // Fard prayers - Azan
                    new() { PrayerName = "fajr_azan", PrayerTimeValue = new TimeSpan(5, 0, 0), DisplayNameEn = "Fajr Azan", DisplayNameBn = "ফজর আযান", Category = "fard", PrayerType = "azan", DisplayOrder = 1, IsActive = true },
                    new() { PrayerName = "dhuhr_azan", PrayerTimeValue = new TimeSpan(12, 15, 0), DisplayNameEn = "Dhuhr Azan", DisplayNameBn = "যোহর আযান", Category = "fard", PrayerType = "azan", DisplayOrder = 3, IsActive = true },
                    new() { PrayerName = "asr_azan", PrayerTimeValue = new TimeSpan(15, 45, 0), DisplayNameEn = "Asr Azan", DisplayNameBn = "আসর আযান", Category = "fard", PrayerType = "azan", DisplayOrder = 5, IsActive = true },
                    new() { PrayerName = "maghrib_azan", PrayerTimeValue = new TimeSpan(18, 30, 0), DisplayNameEn = "Maghrib Azan", DisplayNameBn = "মাগরিব আযান", Category = "fard", PrayerType = "azan", DisplayOrder = 7, IsActive = true },
                    new() { PrayerName = "isha_azan", PrayerTimeValue = new TimeSpan(20, 0, 0), DisplayNameEn = "Isha Azan", DisplayNameBn = "ইশা আযান", Category = "fard", PrayerType = "azan", DisplayOrder = 9, IsActive = true },

                    // Fard prayers - Jamaat
                    new() { PrayerName = "fajr_jamaat", PrayerTimeValue = new TimeSpan(5, 30, 0), DisplayNameEn = "Fajr Jamaat", DisplayNameBn = "ফজর জামাত", Category = "fard", PrayerType = "jamaat", DisplayOrder = 2, IsActive = true },
                    new() { PrayerName = "dhuhr_jamaat", PrayerTimeValue = new TimeSpan(12, 30, 0), DisplayNameEn = "Dhuhr Jamaat", DisplayNameBn = "যোহর জামাত", Category = "fard", PrayerType = "jamaat", DisplayOrder = 4, IsActive = true },
                    new() { PrayerName = "asr_jamaat", PrayerTimeValue = new TimeSpan(16, 0, 0), DisplayNameEn = "Asr Jamaat", DisplayNameBn = "আসর জামাত", Category = "fard", PrayerType = "jamaat", DisplayOrder = 6, IsActive = true },
                    new() { PrayerName = "maghrib_jamaat", PrayerTimeValue = new TimeSpan(18, 35, 0), DisplayNameEn = "Maghrib Jamaat", DisplayNameBn = "মাগরিব জামাত", Category = "fard", PrayerType = "jamaat", DisplayOrder = 8, IsActive = true },
                    new() { PrayerName = "isha_jamaat", PrayerTimeValue = new TimeSpan(20, 15, 0), DisplayNameEn = "Isha Jamaat", DisplayNameBn = "ইশা জামাত", Category = "fard", PrayerType = "jamaat", DisplayOrder = 10, IsActive = true },

                    // Nafl prayers
                    new() { PrayerName = "tahajjut", PrayerTimeValue = new TimeSpan(2, 30, 0), DisplayNameEn = "Tahajjut", DisplayNameBn = "তাহাজ্জুদ", Category = "nafl", PrayerType = "optional", DisplayOrder = 11, IsActive = true },
                    new() { PrayerName = "ishraq", PrayerTimeValue = new TimeSpan(5, 45, 0), DisplayNameEn = "Ishraq", DisplayNameBn = "ইশরাক", Category = "nafl", PrayerType = "optional", DisplayOrder = 12, IsActive = true },
                    new() { PrayerName = "duha", PrayerTimeValue = new TimeSpan(8, 0, 0), DisplayNameEn = "Duha (Chasht)", DisplayNameBn = "দুহা (চাশত)", Category = "nafl", PrayerType = "optional", DisplayOrder = 13, IsActive = true },
                    new() { PrayerName = "awwabin", PrayerTimeValue = new TimeSpan(18, 45, 0), DisplayNameEn = "Awwabin", DisplayNameBn = "আওয়াবীন", Category = "nafl", PrayerType = "optional", DisplayOrder = 14, IsActive = true }
                };

                await context.PrayerTimes.AddRangeAsync(prayerTimes);
                await context.SaveChangesAsync();
            }

            // 5. Seed Islamic Events
            if (!await context.IslamicEvents.AnyAsync())
            {
                var events = new List<IslamicEvent>
                {
                    new() { EventName = "Ramadan 2026", EventDate = DateTime.Today.AddDays(10), HijriDate = "1 Ramadan 1447h", HijriMonth = "Ramadan", HijriDay = 1, EventType = "religious", Description = "First day of the holy month of Ramadan, fasting and special nightly Taraweeh prayers.", DisplayOrder = 1, IsActive = true },
                    new() { EventName = "Laylat al Qadr 2026", EventDate = DateTime.Today.AddDays(36), HijriDate = "27 Ramadan 1447h", HijriMonth = "Ramadan", HijriDay = 27, EventType = "special", Description = "The Night of Power, better than a thousand months. Qiyam-ul-layl and special Quran Khatam Dua.", DisplayOrder = 2, IsActive = true },
                    new() { EventName = "Eid ul Fitr 2026", EventDate = DateTime.Today.AddDays(40), HijriDate = "1 Shawwal 1447h", HijriMonth = "Shawwal", HijriDay = 1, EventType = "festival", Description = "Grand Festival of Breaking the Fast. 3 Jamaat shifts starting from 7:00 AM.", DisplayOrder = 3, IsActive = true },
                    new() { EventName = "Day of Arafah & Hajj 2026", EventDate = DateTime.Today.AddDays(105), HijriDate = "9 Dhul Hijjah 1447h", HijriMonth = "Dhul Hijjah", HijriDay = 9, EventType = "religious", Description = "Day of Arafah, key pillar of Hajj and recommended day of Sunnah fasting.", DisplayOrder = 4, IsActive = true },
                    new() { EventName = "Eid al Adha 2026", EventDate = DateTime.Today.AddDays(106), HijriDate = "10 Dhul Hijjah 1447h", HijriMonth = "Dhul Hijjah", HijriDay = 10, EventType = "festival", Description = "Festival of Sacrifice (Qurbani). Collective Qurbani and meat distribution program.", DisplayOrder = 5, IsActive = true },
                    new() { EventName = "Islamic New Year (1448 Hijri)", EventDate = DateTime.Today.AddDays(125), HijriDate = "1 Muharram 1448h", HijriMonth = "Muharram", HijriDay = 1, EventType = "religious", Description = "Start of the new Islamic Hijri Year 1448.", DisplayOrder = 6, IsActive = true },
                    new() { EventName = "Day of Ashura", EventDate = DateTime.Today.AddDays(134), HijriDate = "10 Muharram 1448h", HijriMonth = "Muharram", HijriDay = 10, EventType = "historical", Description = "Day of Ashura commemorating historical milestones and the martyrdom of Hazrat Hussain (RA).", DisplayOrder = 7, IsActive = true },
                    new() { EventName = "Milad-un-Nabi (12 Rabi ul Awal)", EventDate = DateTime.Today.AddDays(195), HijriDate = "12 Rabi ul Awal 1448h", HijriMonth = "Rabi ul Awal", HijriDay = 12, EventType = "festival", Description = "Commemoration of the birth and legacy of Prophet Muhammad (PBUH) with Seerat conferences.", DisplayOrder = 8, IsActive = true }
                };

                await context.IslamicEvents.AddRangeAsync(events);
                await context.SaveChangesAsync();
            }

            // 6. Seed Activities
            if (!await context.Activities.AnyAsync())
            {
                var activities = new List<Activity>
                {
                    new()
                    {
                        Title = "Daily Quran & Tajweed Classes for Youth",
                        Description = "Structured evening classes teaching Quranic recitation, proper Tajweed rules, and basic Islamic manners (Adab) for children and teenagers.",
                        Category = "education",
                        ImageUrl = "https://images.unsplash.com/photo-1609599006353-e629aaabfeae?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "Community Zakat & Food Relief Program",
                        Description = "Monthly distribution of essential food baskets, medical subsidies, and micro-grant empowerments to impoverished local families funded by transparent community Zakat.",
                        Category = "charity",
                        ImageUrl = "https://images.unsplash.com/photo-1593113598332-cd288d649433?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "Winter Warmth Clothing Drive (Sitarto)",
                        Description = "Annual initiative distributing high-quality blankets and warm winter clothing sets to underprivileged families in rural and peri-urban areas.",
                        Category = "charity",
                        ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 3,
                        IsActive = true
                    },
                    new()
                    {
                        Title = "Green Mosque Tree Plantation (Gachropon)",
                        Description = "Environmental sustainability drive in line with SDG 9, planting fruit and shade trees across surrounding community areas to promote eco-friendly infrastructure.",
                        Category = "community",
                        ImageUrl = "https://images.unsplash.com/photo-1542601906990-b4d3fb778b09?auto=format&fit=crop&w=800&q=80",
                        DisplayOrder = 4,
                        IsActive = true
                    }
                };

                await context.Activities.AddRangeAsync(activities);
                await context.SaveChangesAsync();
            }

            // 7. Seed Sample Donations
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

            // 8. Seed Sample Milad Requests
            if (!await context.MiladRequests.AnyAsync() && testUser != null)
            {
                var sampleMilads = new List<MiladRequest>
                {
                    new()
                    {
                        UserId = testUser.Id,
                        Name = "Dani Family Annual Shukrana Milad",
                        Phone = "+8801811111111",
                        Description = "Arranging a family Shukrana Milad & Dua Mehfil for mother's health recovery and deceased grandparents.",
                        MiladDate = DateTime.Today.AddDays(5),
                        Status = "approved",
                        AdminRemark = "Approved. Maulana Abdur Rahman has been assigned to lead the Dua Mehfil after Maghrib.",
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                };

                await context.MiladRequests.AddRangeAsync(sampleMilads);
                await context.SaveChangesAsync();
            }
        }
    }
}
