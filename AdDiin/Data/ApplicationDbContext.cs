using AdDiin.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PrayerTime> PrayerTimes => Set<PrayerTime>();
        public DbSet<IslamicEvent> IslamicEvents => Set<IslamicEvent>();
        public DbSet<MiladRequest> MiladRequests => Set<MiladRequest>();
        public DbSet<Donation> Donations => Set<Donation>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<ProgramRegistration> ProgramRegistrations => Set<ProgramRegistration>();
        public DbSet<DhikrRecord> DhikrRecords => Set<DhikrRecord>();
        public DbSet<QuranReadingLog> QuranReadingLogs => Set<QuranReadingLog>();
        public DbSet<AdhkarLog> AdhkarLogs => Set<AdhkarLog>();
        public DbSet<RuqyahLog> RuqyahLogs => Set<RuqyahLog>();
        public DbSet<DailyDeenGoal> DailyDeenGoals => Set<DailyDeenGoal>();
        public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
        public DbSet<UserDeenSettings> UserDeenSettings => Set<UserDeenSettings>();
        public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Conversation relationships with NoAction/Restrict to prevent SQL Server cascade cycle
            builder.Entity<Conversation>()
                .HasOne(c => c.User)
                .WithMany(u => u.UserConversations)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(c => c.Admin)
                .WithMany(u => u.AdminConversations)
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Message relationships
            builder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProgramRegistration relationship
            builder.Entity<ProgramRegistration>()
                .HasOne(r => r.Activity)
                .WithMany(a => a.Registrations)
                .HasForeignKey(r => r.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProgramRegistration>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProgramRegistration>()
                .HasIndex(r => r.Status);

            // Configure MiladRequest relationship
            builder.Entity<MiladRequest>()
                .HasOne(m => m.User)
                .WithMany(u => u.MiladRequests)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MiladRequest>()
                .HasIndex(m => m.Status);

            // Configure Donation relationship
            builder.Entity<Donation>()
                .HasOne(d => d.User)
                .WithMany(u => u.Donations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Donation>()
                .HasIndex(d => d.TranId)
                .IsUnique();

            builder.Entity<Donation>()
                .HasIndex(d => d.PaymentStatus);

            builder.Entity<Donation>()
                .HasIndex(d => d.Category);

            // Configure PrayerTime
            builder.Entity<PrayerTime>()
                .HasIndex(p => p.PrayerName)
                .IsUnique();

            builder.Entity<PrayerTime>()
                .HasIndex(p => p.DisplayOrder);

            // Configure VerificationCode
            builder.Entity<VerificationCode>()
                .HasIndex(v => v.Email);
        }
    }
}