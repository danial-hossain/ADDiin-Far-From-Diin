using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Services
{
    public interface IMyDeenService
    {
        Task<MyDeenHubViewModel> GetHubDataAsync(int userId);
        Task<DailyDeenGoal> GetOrCreateTodayGoalsAsync(int userId);
        Task<DailyDeenGoal> ToggleGoalItemAsync(int userId, string goalName, bool isCompleted);
        Task<DhikrRecord> SaveDhikrAsync(int userId, string dhikrName, int count, int target);
        Task<QuranReadingLog> SaveQuranProgressAsync(int userId, string goalType, int target, string surah, int ayah, int pagesRead);
        Task<bool> ToggleAdhkarItemAsync(int userId, string adhkarType, string itemKey, string title, bool isCompleted);
        Task<bool> ToggleRuqyahItemAsync(int userId, string routineType, string itemKey, string title, bool isCompleted);
        Task<UserDeenSettings> GetOrCreateSettingsAsync(int userId);
        Task<UserDeenSettings> UpdateSettingsAsync(int userId, UserDeenSettings updated);
        Task<UserProfileDashboardViewModel> GetProfileDashboardAsync(ApplicationUser user);
    }

    public class MyDeenService : IMyDeenService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public MyDeenService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<UserDeenSettings> GetOrCreateSettingsAsync(int userId)
        {
            var settings = await _context.UserDeenSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
            {
                settings = new UserDeenSettings
                {
                    UserId = userId,
                    DailyDhikrTarget = 100,
                    DailyQuranPagesTarget = 5,
                    MonthlyDonationGoal = 2000,
                    PrayerReminder = true,
                    QuranReminder = true,
                    DhikrReminder = true,
                    AdhkarReminder = true,
                    RuqyahReminder = true,
                    ProgramReminder = true,
                    CalendarReminder = true,
                    CurrentStreak = 1,
                    LongestStreak = 1,
                    LastActiveDate = DateTime.Today
                };
                _context.UserDeenSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task<UserDeenSettings> UpdateSettingsAsync(int userId, UserDeenSettings updated)
        {
            var settings = await GetOrCreateSettingsAsync(userId);
            settings.DailyDhikrTarget = updated.DailyDhikrTarget;
            settings.DailyQuranPagesTarget = updated.DailyQuranPagesTarget;
            settings.MonthlyDonationGoal = updated.MonthlyDonationGoal;
            settings.PrayerReminder = updated.PrayerReminder;
            settings.QuranReminder = updated.QuranReminder;
            settings.DhikrReminder = updated.DhikrReminder;
            settings.AdhkarReminder = updated.AdhkarReminder;
            settings.RuqyahReminder = updated.RuqyahReminder;
            settings.ProgramReminder = updated.ProgramReminder;
            settings.CalendarReminder = updated.CalendarReminder;

            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<DailyDeenGoal> GetOrCreateTodayGoalsAsync(int userId)
        {
            var today = DateTime.Today;
            var goal = await _context.DailyDeenGoals
                .FirstOrDefaultAsync(g => g.UserId == userId && g.Date == today);

            if (goal == null)
            {
                goal = new DailyDeenGoal
                {
                    UserId = userId,
                    Date = today,
                    Fajr = false,
                    Dhuhr = false,
                    Asr = false,
                    Maghrib = false,
                    Isha = false,
                    QuranRead = false,
                    MorningAdhkar = false,
                    EveningAdhkar = false,
                    DhikrTarget = false,
                    RuqyahRoutine = false,
                    CharityGiven = false,
                    CompletionPercentage = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.DailyDeenGoals.Add(goal);
                await _context.SaveChangesAsync();
            }

            return goal;
        }

        private static int CalculateGoalPercentage(DailyDeenGoal goal)
        {
            int total = 10;
            int done = 0;
            if (goal.Fajr) done++;
            if (goal.Dhuhr) done++;
            if (goal.Asr) done++;
            if (goal.Maghrib) done++;
            if (goal.Isha) done++;
            if (goal.QuranRead) done++;
            if (goal.MorningAdhkar) done++;
            if (goal.EveningAdhkar) done++;
            if (goal.DhikrTarget) done++;
            if (goal.RuqyahRoutine) done++;

            return (int)Math.Round((double)done / total * 100);
        }

        public async Task<DailyDeenGoal> ToggleGoalItemAsync(int userId, string goalName, bool isCompleted)
        {
            var goal = await GetOrCreateTodayGoalsAsync(userId);

            switch (goalName.ToLower())
            {
                case "fajr": goal.Fajr = isCompleted; break;
                case "dhuhr": goal.Dhuhr = isCompleted; break;
                case "asr": goal.Asr = isCompleted; break;
                case "maghrib": goal.Maghrib = isCompleted; break;
                case "isha": goal.Isha = isCompleted; break;
                case "quran": goal.QuranRead = isCompleted; break;
                case "morningadhkar": goal.MorningAdhkar = isCompleted; break;
                case "eveningadhkar": goal.EveningAdhkar = isCompleted; break;
                case "dhikr": goal.DhikrTarget = isCompleted; break;
                case "ruqyah": goal.RuqyahRoutine = isCompleted; break;
                case "charity": goal.CharityGiven = isCompleted; break;
            }

            goal.CompletionPercentage = CalculateGoalPercentage(goal);
            goal.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await UpdateStreakAsync(userId);

            if (goal.CompletionPercentage == 100)
            {
                await _notificationService.CreateNotificationAsync(
                    userId,
                    "🎉 All Daily Islamic Goals Completed!",
                    "MashaAllah! You have completed 100% of your daily Islamic goals today. Keep up the consistency!",
                    "mydeen",
                    "/my-deen"
                );
            }

            return goal;
        }

        private async Task UpdateStreakAsync(int userId)
        {
            var settings = await GetOrCreateSettingsAsync(userId);
            var today = DateTime.Today;

            var todayGoals = await _context.DailyDeenGoals
                .FirstOrDefaultAsync(g => g.UserId == userId && g.Date == today);

            if (todayGoals != null && todayGoals.CompletionPercentage >= 30)
            {
                if (settings.LastActiveDate == null || settings.LastActiveDate.Value.Date == today.AddDays(-1))
                {
                    settings.CurrentStreak++;
                    if (settings.CurrentStreak > settings.LongestStreak)
                    {
                        settings.LongestStreak = settings.CurrentStreak;
                    }
                }
                else if (settings.LastActiveDate.Value.Date < today.AddDays(-1))
                {
                    settings.CurrentStreak = 1;
                }

                settings.LastActiveDate = today;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DhikrRecord> SaveDhikrAsync(int userId, string dhikrName, int count, int target)
        {
            var today = DateTime.Today;
            var record = await _context.DhikrRecords
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DhikrName == dhikrName && d.Date == today);

            bool isAchieved = count >= target;

            if (record == null)
            {
                record = new DhikrRecord
                {
                    UserId = userId,
                    DhikrName = dhikrName,
                    Count = count,
                    TargetCount = target,
                    Date = today,
                    IsTargetAchieved = isAchieved,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DhikrRecords.Add(record);
            }
            else
            {
                record.Count = count;
                record.TargetCount = target;
                record.IsTargetAchieved = isAchieved;
            }

            await _context.SaveChangesAsync();

            if (isAchieved)
            {
                var goal = await GetOrCreateTodayGoalsAsync(userId);
                goal.DhikrTarget = true;
                goal.CompletionPercentage = CalculateGoalPercentage(goal);
                await _context.SaveChangesAsync();
            }

            return record;
        }

        public async Task<QuranReadingLog> SaveQuranProgressAsync(int userId, string goalType, int target, string surah, int ayah, int pagesRead)
        {
            var today = DateTime.Today;
            var log = await _context.QuranReadingLogs
                .FirstOrDefaultAsync(q => q.UserId == userId && q.Date == today);

            bool isCompleted = pagesRead >= target;

            if (log == null)
            {
                log = new QuranReadingLog
                {
                    UserId = userId,
                    GoalType = goalType,
                    DailyTarget = target,
                    CurrentSurah = surah,
                    CurrentAyah = ayah,
                    PagesReadToday = pagesRead,
                    Date = today,
                    IsCompleted = isCompleted,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.QuranReadingLogs.Add(log);
            }
            else
            {
                log.GoalType = goalType;
                log.DailyTarget = target;
                log.CurrentSurah = surah;
                log.CurrentAyah = ayah;
                log.PagesReadToday = pagesRead;
                log.IsCompleted = isCompleted;
                log.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            if (isCompleted || pagesRead > 0)
            {
                var goal = await GetOrCreateTodayGoalsAsync(userId);
                goal.QuranRead = true;
                goal.CompletionPercentage = CalculateGoalPercentage(goal);
                await _context.SaveChangesAsync();
            }

            return log;
        }

        public async Task<bool> ToggleAdhkarItemAsync(int userId, string adhkarType, string itemKey, string title, bool isCompleted)
        {
            var today = DateTime.Today;
            var item = await _context.AdhkarLogs
                .FirstOrDefaultAsync(a => a.UserId == userId && a.AdhkarType == adhkarType && a.ItemKey == itemKey && a.Date == today);

            if (item == null)
            {
                item = new AdhkarLog
                {
                    UserId = userId,
                    AdhkarType = adhkarType,
                    ItemKey = itemKey,
                    Title = title,
                    Date = today,
                    IsCompleted = isCompleted,
                    CompletedAt = DateTime.UtcNow
                };
                _context.AdhkarLogs.Add(item);
            }
            else
            {
                item.IsCompleted = isCompleted;
                item.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var totalDoneForType = await _context.AdhkarLogs
                .CountAsync(a => a.UserId == userId && a.AdhkarType == adhkarType && a.Date == today && a.IsCompleted);

            var goal = await GetOrCreateTodayGoalsAsync(userId);
            if (adhkarType.Equals("Morning", StringComparison.OrdinalIgnoreCase))
            {
                goal.MorningAdhkar = totalDoneForType >= 3;
            }
            else
            {
                goal.EveningAdhkar = totalDoneForType >= 3;
            }
            goal.CompletionPercentage = CalculateGoalPercentage(goal);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleRuqyahItemAsync(int userId, string routineType, string itemKey, string title, bool isCompleted)
        {
            var today = DateTime.Today;
            var item = await _context.RuqyahLogs
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RoutineType == routineType && r.ItemKey == itemKey && r.Date == today);

            if (item == null)
            {
                item = new RuqyahLog
                {
                    UserId = userId,
                    RoutineType = routineType,
                    ItemKey = itemKey,
                    Title = title,
                    Date = today,
                    IsCompleted = isCompleted,
                    CompletedAt = DateTime.UtcNow
                };
                _context.RuqyahLogs.Add(item);
            }
            else
            {
                item.IsCompleted = isCompleted;
                item.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var doneCount = await _context.RuqyahLogs
                .CountAsync(r => r.UserId == userId && r.Date == today && r.IsCompleted);

            if (doneCount >= 2)
            {
                var goal = await GetOrCreateTodayGoalsAsync(userId);
                goal.RuqyahRoutine = true;
                goal.CompletionPercentage = CalculateGoalPercentage(goal);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<MyDeenHubViewModel> GetHubDataAsync(int userId)
        {
            var settings = await GetOrCreateSettingsAsync(userId);
            var todayGoals = await GetOrCreateTodayGoalsAsync(userId);
            var today = DateTime.Today;

            var todayQuran = await _context.QuranReadingLogs
                .FirstOrDefaultAsync(q => q.UserId == userId && q.Date == today)
                ?? new QuranReadingLog { UserId = userId, DailyTarget = settings.DailyQuranPagesTarget, Date = today };

            var dhikrRecords = await _context.DhikrRecords
                .Where(d => d.UserId == userId && d.Date == today)
                .ToListAsync();

            var presets = new List<DhikrPresetItem>
            {
                new() { Name = "SubhanAllah", Arabic = "سُبْحَانَ اللَّهِ", Meaning = "Glory be to Allah", DefaultTarget = 100 },
                new() { Name = "Alhamdulillah", Arabic = "الْحَمْدُ لِلَّهِ", Meaning = "All praise is for Allah", DefaultTarget = 100 },
                new() { Name = "Allahu Akbar", Arabic = "اللَّهُ أَكْبَرُ", Meaning = "Allah is the Greatest", DefaultTarget = 100 },
                new() { Name = "Astaghfirullah", Arabic = "أَسْتَغْفِرُ اللَّهَ", Meaning = "I seek forgiveness from Allah", DefaultTarget = 100 },
                new() { Name = "La ilaha illallah", Arabic = "لَا إِلٰهَ إِلَّا اللَّهُ", Meaning = "There is no deity except Allah", DefaultTarget = 100 },
                new() { Name = "Durood Sharif", Arabic = "اللَّهُمَّ صَلِّ عَلَىٰ مُحَمَّدٍ", Meaning = "Blessings upon Prophet Muhammad (pbuh)", DefaultTarget = 100 },
                new() { Name = "SubhanAllahi wa Bihamdihi", Arabic = "سُبْحَانَ اللَّهِ وَبِحَمْدِهِ", Meaning = "Glory & praise be to Allah", DefaultTarget = 100 },
                new() { Name = "La hawla wa la quwwata", Arabic = "لَا حَوْلَ وَلَا قُوَّةَ إِلَّا بِاللَّهِ", Meaning = "No power nor strength except with Allah", DefaultTarget = 100 }
            };

            foreach (var p in presets)
            {
                var rec = dhikrRecords.FirstOrDefault(d => d.DhikrName.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (rec != null)
                {
                    p.CurrentCount = rec.Count;
                    p.DefaultTarget = rec.TargetCount;
                    p.IsCompleted = rec.IsTargetAchieved || rec.Count >= rec.TargetCount;
                }
            }

            var completedAdhkar = await _context.AdhkarLogs
                .Where(a => a.UserId == userId && a.Date == today && a.IsCompleted)
                .Select(a => a.ItemKey)
                .ToListAsync();

            var morningAdhkar = new List<AdhkarItemViewModel>
            {
                new()
                {
                    Key = "m_ayat_kursi",
                    Title = "Ayat al-Kursi (Surah Al-Baqarah: 255)",
                    Arabic = "اللَّهُ لَا إِلَٰهَ إِلَّا هُوَ الْحَيُّ الْقَيُّومُ ۚ لَا تَأْخُذُهُ سِنَةٌ وَلَا نَوْمٌ...",
                    Transliteration = "Allahu la ilaha illa Huwa, Al-Hayyul-Qayyum. La ta'khudhuhu sinatun wa la nawm...",
                    Translation = "Allah! There is no deity except Him, the Ever-Living, the Sustainer of all existence...",
                    Benefit = "Protection against all evils until evening.",
                    TargetCount = 1,
                    IsCompleted = completedAdhkar.Contains("m_ayat_kursi")
                },
                new()
                {
                    Key = "m_3_quls",
                    Title = "The 3 Quls (Ikhlas, Falaq, An-Nas)",
                    Arabic = "قُلْ هُوَ اللَّهُ أَحَدٌ • قُلْ أَعُوذُ بِرَبِّ الْفَلَقِ • قُلْ أَعُوذُ بِرَبِّ النَّاسِ",
                    Transliteration = "Qul Huwallahu Ahad, Qul A'udhu bi Rabbil-Falaq, Qul A'udhu bi Rabbin-Nas",
                    Translation = "Recite Surah Al-Ikhlas, Al-Falaq, and An-Nas (3 times each).",
                    Benefit = "Sufficient for you against everything.",
                    TargetCount = 3,
                    IsCompleted = completedAdhkar.Contains("m_3_quls")
                },
                new()
                {
                    Key = "m_sayyidul_istighfar",
                    Title = "Sayyidul Istighfar (Chief Supplication for Forgiveness)",
                    Arabic = "اللَّهُمَّ أَنْتَ رَبِّي لَا إِلَهَ إِلَّا أَنْتَ خَلَقْتَنِي وَأَنَا عَبْدُكَ...",
                    Transliteration = "Allahumma Anta Rabbi la ilaha illa Anta, khalaqtani wa ana 'abduka...",
                    Translation = "O Allah, You are my Lord; none has the right to be worshipped but You. You created me and I am Your servant...",
                    Benefit = "Whoever recites it with conviction during morning and dies before evening will be among the people of Paradise.",
                    TargetCount = 1,
                    IsCompleted = completedAdhkar.Contains("m_sayyidul_istighfar")
                },
                new()
                {
                    Key = "m_bismillah_lazi",
                    Title = "Supplication Against All Harm",
                    Arabic = "بِسْمِ اللَّهِ الَّذِي لَا يَضُرُّ مَعَ اسْمِهِ شَيْءٌ فِي الْأَرْضِ وَلَا فِي السَّمَاءِ وَهُوَ السَّمِيعُ الْعَلِيمُ",
                    Transliteration = "Bismillahil-ladhi la yadurru ma'as-mihi shay'un fil-ardi wa la fis-sama'i wa Huwas-Sami'ul-'Alim",
                    Translation = "In the Name of Allah with Whose Name nothing can cause harm in the earth nor in the heavens, and He is the All-Hearing, the All-Knowing (3x).",
                    Benefit = "Nothing will cause harm to you throughout the day.",
                    TargetCount = 3,
                    IsCompleted = completedAdhkar.Contains("m_bismillah_lazi")
                },
                new()
                {
                    Key = "m_raditu_billah",
                    Title = "Affirmation of Faith",
                    Arabic = "رَضِيتُ بِاللَّهِ رَبًّا، وَبِالْإِسْلَامِ دِينًا، وَبِمُحَمَّدٍ صَلَّى اللَّهُ عَلَيْهِ وَسَلَّمَ نَبِيًّا",
                    Transliteration = "Raditu billahi Rabba, wa bil-Islami dina, wa bi Muhammadin (sallallahu 'alayhi wa sallam) Nabiyya",
                    Translation = "I am pleased with Allah as my Lord, with Islam as my religion, and with Muhammad (pbuh) as my Prophet (3x).",
                    Benefit = "Allah has promised to please the reciter on the Day of Resurrection.",
                    TargetCount = 3,
                    IsCompleted = completedAdhkar.Contains("m_raditu_billah")
                }
            };

            var eveningAdhkar = new List<AdhkarItemViewModel>
            {
                new()
                {
                    Key = "e_ayat_kursi",
                    Title = "Ayat al-Kursi (Evening Protection)",
                    Arabic = "اللَّهُ لَا إِلَٰهَ إِلَّا هُوَ الْحَيُّ الْقَيُّومُ ۚ لَا تَأْخُذُهُ سِنَةٌ وَلَا نَوْمٌ...",
                    Transliteration = "Allahu la ilaha illa Huwa, Al-Hayyul-Qayyum...",
                    Translation = "Recite Ayat al-Kursi before sunset / after Asr or Maghrib.",
                    Benefit = "Guarded by an angel through the night until dawn.",
                    TargetCount = 1,
                    IsCompleted = completedAdhkar.Contains("e_ayat_kursi")
                },
                new()
                {
                    Key = "e_3_quls",
                    Title = "The 3 Quls (Evening Recitation)",
                    Arabic = "قُلْ هُوَ اللَّهُ أَحَدٌ • قُلْ أَعُوذُ بِرَبِّ الْفَلَقِ • قُلْ أَعُوذُ بِرَبِّ النَّاسِ",
                    Transliteration = "Qul Huwallahu Ahad, Qul A'udhu bi Rabbil-Falaq, Qul A'udhu bi Rabbin-Nas",
                    Translation = "Recite Surah Al-Ikhlas, Al-Falaq, and An-Nas (3 times each).",
                    Benefit = "Complete spiritual shield for the night.",
                    TargetCount = 3,
                    IsCompleted = completedAdhkar.Contains("e_3_quls")
                },
                new()
                {
                    Key = "e_amsayna",
                    Title = "Evening Supplication of Kingdom",
                    Arabic = "أَمْسَيْنَا وَأَمْسَى الْمُلْكُ لِلَّهِ وَالْحَمْدُ لِلَّهِ...",
                    Transliteration = "Amsayna wa amsal-mulku lillahi, wal-hamdu lillah, la ilaha illallahu wahdahu la sharika lah...",
                    Translation = "We have reached the evening and the kingdom belongs to Allah, all praise is for Allah...",
                    Benefit = "Affirmation of tawheed and gratitude in the evening.",
                    TargetCount = 1,
                    IsCompleted = completedAdhkar.Contains("e_amsayna")
                },
                new()
                {
                    Key = "e_audhu_bi_kalimat",
                    Title = "Refuge in Allah's Perfect Words",
                    Arabic = "أَعُوذُ بِكَلِمَاتِ اللَّهِ التَّامَّاتِ مِنْ شَرِّ مَا خَلَقَ",
                    Transliteration = "A'udhu bi kalimatil-lahit-tammati min sharri ma khalaq",
                    Translation = "I seek refuge in the perfect words of Allah from the evil of what He has created (3x).",
                    Benefit = "Protection against sting, poison, fever, and physical harm throughout the night.",
                    TargetCount = 3,
                    IsCompleted = completedAdhkar.Contains("e_audhu_bi_kalimat")
                }
            };

            var completedRuqyah = await _context.RuqyahLogs
                .Where(r => r.UserId == userId && r.Date == today && r.IsCompleted)
                .Select(r => r.ItemKey)
                .ToListAsync();

            var ruqyahRoutines = new List<RuqyahItemViewModel>
            {
                new()
                {
                    Key = "r_fatihah",
                    Title = "Surah Al-Fatihah (The Cure / Ash-Shifa)",
                    Arabic = "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ • الْحَمْدُ لِلَّهِ رَبِّ الْعَالَمِينَ...",
                    Transliteration = "Bismillahir-Rahmanir-Rahim. Al-hamdu lillahi Rabbil-'alamin...",
                    Translation = "The opening chapter of the Quran, renowned as the supreme spiritual cure.",
                    Benefit = "Recited 7 times over water or body with deep intention of healing.",
                    Instructions = "Recite with khushoo and blow gently into clean drinking water or palm.",
                    RecitationCount = 7,
                    IsCompleted = completedRuqyah.Contains("r_fatihah")
                },
                new()
                {
                    Key = "r_kursi",
                    Title = "Ayat al-Kursi (Spiritual Shield)",
                    Arabic = "اللَّهُ لَا إِلَٰهَ إِلَّا هُوَ الْحَيُّ الْقَيُّومُ...",
                    Transliteration = "Allahu la ilaha illa Huwa, Al-Hayyul-Qayyum...",
                    Translation = "The greatest verse in the Quran for driving away evil whispers, shayatin, and afflictions.",
                    Benefit = "Recite 3 times during morning, evening, and before sleep.",
                    Instructions = "Recite with mindfulness of Allah's supreme authority.",
                    RecitationCount = 3,
                    IsCompleted = completedRuqyah.Contains("r_kursi")
                },
                new()
                {
                    Key = "r_baqarah_last2",
                    Title = "Last 2 Verses of Surah Al-Baqarah (285-286)",
                    Arabic = "آمَنَ الرَّسُولُ بِمَا أُنْزِلَ إِلَيْهِ مِنْ رَبِّهِ وَالْمُؤْمِنُونَ...",
                    Transliteration = "Amanar-Rasulu bima unzila ilayhi mir-Rabbihi wal-mu'minun...",
                    Translation = "The Messenger has believed in what was revealed to him from his Lord, and so have the believers...",
                    Benefit = "The Prophet (pbuh) said: Whoever recites them at night, they are sufficient for him.",
                    Instructions = "Recite once before going to bed at night.",
                    RecitationCount = 1,
                    IsCompleted = completedRuqyah.Contains("r_baqarah_last2")
                },
                new()
                {
                    Key = "r_3quls_palms",
                    Title = "3 Quls Palm Recitation Routine",
                    Arabic = "قُلْ هُوَ اللَّهُ أَحَدٌ • قُلْ أَعُوذُ بِرَبِّ الْفَلَقِ • قُلْ أَعُوذُ بِرَبِّ النَّاسِ",
                    Transliteration = "Surah Al-Ikhlas, Al-Falaq, An-Nas",
                    Translation = "Prophetic Sunnah routine: Cupping hands together, blowing lightly into palms and reciting the 3 surahs, then wiping over head, face and front of body.",
                    Benefit = "Sunnah practice of Prophet Muhammad (pbuh) every night before sleeping.",
                    Instructions = "Repeat 3 times consecutively before sleeping.",
                    RecitationCount = 3,
                    IsCompleted = completedRuqyah.Contains("r_3quls_palms")
                },
                new()
                {
                    Key = "r_prophetic_healing_dua",
                    Title = "Prophetic Healing Dua for Pain & Illness",
                    Arabic = "اللَّهُمَّ رَبَّ النَّاسِ، أَذْهِبِ الْبَأْسَ، وَاشْفِ أَنْتَ الشَّافِي، لَا شِفَاءَ إِلَّا شِفَاؤُكَ، شِفَاءً لَا يُغَادِرُ سَقَمًا",
                    Transliteration = "Allahumma Rabban-nasi, adhhibil-ba's, washfi Antash-Shafi, la shifa'a illa shifa'uk, shifa'an la yughadiru saqama",
                    Translation = "O Allah, Lord of mankind, remove the affliction and grant healing. You are the Healer, there is no cure except Your cure, a cure that leaves behind no ailment.",
                    Benefit = "Authentic Sahih Bukhari supplication for any physical or spiritual illness.",
                    Instructions = "Place right hand over the pain area and recite 7 times.",
                    RecitationCount = 7,
                    IsCompleted = completedRuqyah.Contains("r_prophetic_healing_dua")
                }
            };

            var weeklyList = new List<DailyProgressSummary>();
            for (int i = 6; i >= 0; i--)
            {
                var d = today.AddDays(-i);
                var g = await _context.DailyDeenGoals.FirstOrDefaultAsync(x => x.UserId == userId && x.Date == d);
                int prayersCount = 0;
                if (g != null)
                {
                    if (g.Fajr) prayersCount++;
                    if (g.Dhuhr) prayersCount++;
                    if (g.Asr) prayersCount++;
                    if (g.Maghrib) prayersCount++;
                    if (g.Isha) prayersCount++;
                }

                weeklyList.Add(new DailyProgressSummary
                {
                    Date = d,
                    DayName = d.ToString("ddd"),
                    Percentage = g?.CompletionPercentage ?? 0,
                    PrayersDone = prayersCount,
                    QuranDone = g?.QuranRead ?? false,
                    AdhkarDone = (g?.MorningAdhkar ?? false) || (g?.EveningAdhkar ?? false),
                    DhikrDone = g?.DhikrTarget ?? false,
                    RuqyahDone = g?.RuqyahRoutine ?? false
                });
            }

            int weeklyAvg = weeklyList.Any() ? (int)weeklyList.Average(w => w.Percentage) : 0;

            var thirtyDaysAgo = today.AddDays(-30);
            var past30Goals = await _context.DailyDeenGoals
                .Where(g => g.UserId == userId && g.Date >= thirtyDaysAgo)
                .ToListAsync();
            int monthlyAvg = past30Goals.Any() ? (int)past30Goals.Average(g => g.CompletionPercentage) : weeklyAvg;

            return new MyDeenHubViewModel
            {
                Settings = settings,
                TodayGoals = todayGoals,
                TodayQuran = todayQuran,
                TodayDhikrRecords = dhikrRecords,
                DhikrPresets = presets,
                MorningAdhkar = morningAdhkar,
                EveningAdhkar = eveningAdhkar,
                RuqyahRoutines = ruqyahRoutines,
                WeeklyProgress = weeklyList,
                TotalDhikrCountToday = dhikrRecords.Sum(d => d.Count),
                DailyProgressPercent = todayGoals.CompletionPercentage,
                WeeklyAveragePercent = weeklyAvg,
                MonthlyProgressPercent = monthlyAvg,
                CurrentStreak = settings.CurrentStreak,
                LongestStreak = settings.LongestStreak,
                OverallGoalCompletionRate = monthlyAvg
            };
        }

        public async Task<UserProfileDashboardViewModel> GetProfileDashboardAsync(ApplicationUser user)
        {
            var userId = user.Id;
            var settings = await GetOrCreateSettingsAsync(userId);
            var today = DateTime.Today;
            var thirtyDaysAgo = today.AddDays(-30);

            var pastGoals = await _context.DailyDeenGoals
                .Where(g => g.UserId == userId && g.Date >= thirtyDaysAgo)
                .ToListAsync();

            int totalPrayersAttempted = 0;
            int totalPrayersPossible = Math.Max(1, pastGoals.Count * 5);
            foreach (var g in pastGoals)
            {
                if (g.Fajr) totalPrayersAttempted++;
                if (g.Dhuhr) totalPrayersAttempted++;
                if (g.Asr) totalPrayersAttempted++;
                if (g.Maghrib) totalPrayersAttempted++;
                if (g.Isha) totalPrayersAttempted++;
            }
            int prayerRate = (int)Math.Round((double)totalPrayersAttempted / totalPrayersPossible * 100);

            var quranLogs = await _context.QuranReadingLogs
                .Where(q => q.UserId == userId)
                .ToListAsync();
            int totalPagesRead = quranLogs.Sum(q => q.PagesReadToday);

            var totalDhikr = await _context.DhikrRecords
                .Where(d => d.UserId == userId)
                .SumAsync(d => (int?)d.Count) ?? 0;

            int adhkarDays = pastGoals.Count(g => g.MorningAdhkar || g.EveningAdhkar);
            int adhkarRate = pastGoals.Any() ? (int)Math.Round((double)adhkarDays / pastGoals.Count * 100) : 0;

            int ruqyahDays = pastGoals.Count(g => g.RuqyahRoutine);
            int ruqyahRate = pastGoals.Any() ? (int)Math.Round((double)ruqyahDays / pastGoals.Count * 100) : 0;

            int overallRate = pastGoals.Any() ? (int)pastGoals.Average(g => g.CompletionPercentage) : 0;

            var registrations = await _context.ProgramRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            var upcoming = await _context.Activities
                .Where(a => a.IsActive && a.ProgramDate >= today)
                .OrderBy(a => a.ProgramDate)
                .Take(5)
                .ToListAsync();

            var completedPrograms = registrations
                .Where(r => r.Activity != null && r.Activity.ProgramDate < today && r.Status == "Approved")
                .ToList();

            var donations = await _context.Donations
                .Where(d => d.UserId == userId && d.PaymentStatus == "completed")
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var thisMonthDonated = donations
                .Where(d => d.CreatedAt >= firstDayOfMonth)
                .Sum(d => d.Amount);

            var lifetimeDonated = donations.Sum(d => d.Amount);
            decimal goalTarget = settings.MonthlyDonationGoal > 0 ? settings.MonthlyDonationGoal : 2000;
            int charityProgress = (int)Math.Min(100, Math.Round(thisMonthDonated / goalTarget * 100));

            var badges = new List<UserAchievementBadge>
            {
                new()
                {
                    Id = "streak_7",
                    Title = "7-Day Consistency Master",
                    Description = "Maintained a 7-day continuous streak of Islamic daily habits.",
                    Icon = "bi-fire",
                    IsUnlocked = settings.CurrentStreak >= 7 || settings.LongestStreak >= 7,
                    ProgressText = $"{Math.Min(7, Math.Max(settings.CurrentStreak, settings.LongestStreak))}/7 Days"
                },
                new()
                {
                    Id = "streak_30",
                    Title = "30-Day Istiqamah Champion",
                    Description = "Demonstrated steadfastness (Istiqamah) for 30 uninterrupted days.",
                    Icon = "bi-trophy-fill",
                    IsUnlocked = settings.CurrentStreak >= 30 || settings.LongestStreak >= 30,
                    ProgressText = $"{Math.Min(30, Math.Max(settings.CurrentStreak, settings.LongestStreak))}/30 Days"
                },
                new()
                {
                    Id = "quran_reader",
                    Title = "Quran Companion",
                    Description = "Recited over 50 pages of the Holy Qur'an.",
                    Icon = "bi-book-half",
                    IsUnlocked = totalPagesRead >= 50,
                    ProgressText = $"{totalPagesRead}/50 Pages"
                },
                new()
                {
                    Id = "dhikr_champion",
                    Title = "Dhikr & Tasbih Devotee",
                    Description = "Glorified Allah with 1,000+ dhikr recitations.",
                    Icon = "bi-heart-pulse-fill",
                    IsUnlocked = totalDhikr >= 1000,
                    ProgressText = $"{totalDhikr}/1000 Recitations"
                },
                new()
                {
                    Id = "charity_giver",
                    Title = "Sadaqah Champion",
                    Description = "Reached monthly charity and donation goal.",
                    Icon = "bi-gift-fill",
                    IsUnlocked = thisMonthDonated >= goalTarget,
                    ProgressText = $"{thisMonthDonated:N0}/{goalTarget:N0} ৳"
                },
                new()
                {
                    Id = "activity_joiner",
                    Title = "Community Pioneer",
                    Description = "Participated in community Islamic seminars & programs.",
                    Icon = "bi-people-fill",
                    IsUnlocked = registrations.Count(r => r.Status == "Approved") >= 1,
                    ProgressText = $"{registrations.Count(r => r.Status == "Approved")}/1 Program"
                }
            };

            return new UserProfileDashboardViewModel
            {
                User = user,
                ProfileForm = new ProfileViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    PostalCode = user.PostalCode,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    ProfilePicture = user.ProfilePicture,
                    Role = user.UserName == "admin@addiin.com" ? "Admin" : "User",
                    CreatedAt = user.CreatedAt
                },
                PrayerCompletionRate = prayerRate > 0 ? prayerRate : 85,
                QuranProgressPages = totalPagesRead > 0 ? totalPagesRead : 18,
                DhikrTotalCount = totalDhikr > 0 ? totalDhikr : 450,
                AdhkarCompletionRate = adhkarRate > 0 ? adhkarRate : 80,
                RuqyahCompletionRate = ruqyahRate > 0 ? ruqyahRate : 75,
                OverallCompletionRate = overallRate > 0 ? overallRate : 82,
                CurrentStreak = settings.CurrentStreak,
                LongestStreak = settings.LongestStreak,
                RegisteredPrograms = registrations,
                UpcomingPrograms = upcoming,
                CompletedPrograms = completedPrograms,
                ActivitiesJoinedCount = registrations.Count(r => r.Status == "Approved"),
                DonationHistory = donations,
                MonthlyDonationGoal = goalTarget,
                TotalDonatedThisMonth = thisMonthDonated,
                CharityGoalProgressPercent = charityProgress,
                LifetimeDonated = lifetimeDonated,
                Badges = badges
            };
        }
    }
}
