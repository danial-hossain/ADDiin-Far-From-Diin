# AdDiin Code Trace

This file is meant to help with Git-based review and incremental commits. It explains what each key line in the startup and database configuration code is doing, so later commit history can be followed more clearly.

---

## 1) AdDiin/Program.cs

### Line-by-line explanation

1. `using AdDiin.Data;`  
   Imports the application data layer, including the EF Core database context.

2. `using AdDiin.Hubs;`  
   Imports SignalR hub classes, used for real-time messaging features.

3. `using AdDiin.Models.Entities;`  
   Imports domain models such as users, activities, donations, and message entities.

4. `using AdDiin.Services;`  
   Imports the services that handle business logic like prayer times, donations, AI, and notifications.

5. `using Microsoft.AspNetCore.Identity;`  
   Imports ASP.NET Core Identity for user authentication and authorization.

6. `using Microsoft.EntityFrameworkCore;`  
   Imports EF Core for database configuration and SQL Server connection setup.

7. `var builder = WebApplication.CreateBuilder(args);`  
   Creates the ASP.NET Core app builder and loads configuration from appsettings and environment variables.

8. blank line  
   Makes the file easier to read.

9. `// The fallback keeps local development usable when no connection string has`  
   Comment explaining the purpose of the upcoming database fallback logic.

10. `// been supplied through appsettings, user secrets, or environment variables.`  
   Explains that the connection string may come from multiple configuration sources.

11. `var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")`  
   Reads the configured SQL Server connection string from the app configuration system.

12. `    ?? "Server=(localdb)\\mssqllocaldb;Database=AdDiinDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";`  
   Uses a local SQL Server fallback when no connection string is set, which helps local development run without extra setup.

13. blank line  
   Separates configuration code from service registration.

14. `builder.Services.AddDbContext<ApplicationDbContext>(options =>`  
   Registers the EF Core database context in the dependency injection container.

15. `    options.UseSqlServer(connectionString));`  
   Tells EF Core to use SQL Server as the database provider for the app.

16. blank line  
   Keeps the configuration sections readable.

17. `// Add ASP.NET Core Identity`  
   Comment indicating that Identity services are being configured next.

18. `builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>`  
   Adds Identity to the app with the user and role types used by AdDiin.

19. `{`  
   Opens the identity configuration block.

20. `    options.Password.RequireDigit = false;`  
   Disables password digit requirement, making registration simpler.

21. `    options.Password.RequireLowercase = false;`  
   Disables lowercase requirement.

22. `    options.Password.RequireNonAlphanumeric = false;`  
   Disables special-character requirement.

23. `    options.Password.RequireUppercase = false;`  
   Disables uppercase requirement.

24. `    options.Password.RequiredLength = 6;`  
   Sets minimum password length to 6.

25. `    options.User.RequireUniqueEmail = true;`  
   Ensures users cannot sign up with duplicate emails.

26. `    options.SignIn.RequireConfirmedEmail = false;`  
   Allows sign-in without email confirmation for easier local testing or deployment.

27. `})`  
   Closes the Identity options configuration.

28. `.AddEntityFrameworkStores<ApplicationDbContext>()`  
   Associates Identity data with the project’s database context.

29. `.AddDefaultTokenProviders();`  
   Adds token generation features like password reset and email verification tokens.

30. blank line  
   Break before cookie configuration.

31. `// Configure Cookie Settings`  
   Marks the start of authentication cookie settings.

32. `builder.Services.ConfigureApplicationCookie(options =>`  
   Configures the cookie middleware used for login sessions.

33. `{`  
   Opens cookie settings block.

34. `    options.LoginPath = "/user-login";`  
   Redirects users to the login page when not authenticated.

35. `    options.LogoutPath = "/Account/Logout";`  
   Sets the logout endpoint.

36. `    options.AccessDeniedPath = "/Account/AccessDenied";`  
   Redirects unauthorized users to the access-denied page.

37. `    options.Cookie.HttpOnly = true;`  
   Prevents client-side JavaScript from accessing the cookie.

38. `    options.ExpireTimeSpan = TimeSpan.FromDays(30);`  
   Keeps the login session active for 30 days.

39. `    options.SlidingExpiration = true;`  
   Extends the session each time the user remains active.

40. `});`  
   Ends the cookie configuration.

41. blank line  
   Keeps MVC setup separated.

42. `// Add MVC Services`  
   Indicates the next section registers MVC components.

43. `builder.Services.AddControllersWithViews();`  
   Adds controllers and Razor views to the app.

44. `builder.Services.AddSignalR();`  
   Enables SignalR for real-time chat and related notifications.

45. blank line  
   Makes the feature code block easier to debug.

46. `// Feature branch code — kept for reference only.`  
   Notes that the next block is not currently active.

47. `// It is intentionally inactive.`  
   Clarifies this is retained as historical or optional code.

48. `// builder.Services.AddMemoryCache();`  
   Example of optional in-memory caching that has been disabled for the current branch.

49. blank line  
   Creates spacing before the service registration section.

50. `// Keep domain services scoped so each request receives a consistent unit of`  
   Describes the pattern used for service lifetimes.

51. `// work while HTTP clients remain managed by IHttpClientFactory.`  
   Explains why certain services are scoped while HTTP clients are separate.

52. `builder.Services.AddScoped<IPrayerTimeService, PrayerTimeService>();`  
   Registers the prayer time service as scoped to the request.

53. `builder.Services.AddScoped<IDonationService, DonationService>();`  
   Registers donation logic.

54. `builder.Services.AddScoped<IMiladService, MiladService>();`  
   Registers Milad-related business logic.

55. `builder.Services.AddScoped<IIslamicEventService, IslamicEventService>();`  
   Registers Islamic event service.

56. `builder.Services.AddScoped<IActivityService, ActivityService>();`  
   Registers activity program logic.

57. `builder.Services.AddScoped<IMessagingService, MessagingService>();`  
   Registers messaging logic.

58. `builder.Services.AddHttpClient<IDiinAIService, DiinAIService>((serviceProvider, client) =>`  
   Creates an HTTP client for AI service calls and injects it properly.

59. `{`  
   Opens the AI HTTP client configuration block.

60. `    var config = serviceProvider.GetRequiredService<IConfiguration>();`  
   Reads application configuration for AI settings.

61. `    var timeoutSeconds = config.GetValue<int>("AISettings:TimeoutSeconds", 120);`  
   Reads AI timeout from config with a default of 120 seconds.

62. `    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);`  
   Applies the timeout to external AI requests.

63. `});`  
   Ends the AI client registration.

64. `builder.Services.AddHttpClient<IHalalDetectorService, HalalDetectorService>((serviceProvider, client) =>`  
   Registers the Halal detector API client with a similar configuration pattern.

65. `{`  
   Opens the Halal detector HTTP client setup block.

66. `    var config = serviceProvider.GetRequiredService<IConfiguration>();`  
   Reads config for the service.

67. `    var timeoutSeconds = config.GetValue<int>("AISettings:TimeoutSeconds", 120);`  
   Reuses the same timeout value for consistency.

68. `    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);`  
   Applies the timeout.

69. `});`  
   Ends the Halal detector registration.

70. blank line  
   Creates a separation before the next optional service setup.

71. `// Feature branch Hadith HTTP client configuration — kept for reference only.`  
   Marks the Hadith-specific client block as historical or optional.

72. `// It is intentionally inactive.`  
   Clarifies it is not active in the current branch.

73. `// builder.Services.AddHttpClient<IHadithService, HadithService>(client =>`  
   Shows the earlier Hadith client registration that was disabled.

74. `// {`  
   Opens the commented-out block.

75. `//     client.BaseAddress = new Uri("https://cdn.jsdelivr.net/gh/fawazahmed0/hadith-api@1/");`  
   Example base URL for a public Hadith API.

76. `//     client.Timeout = TimeSpan.FromSeconds(15);`  
   Sets a short timeout for a quick fetch.

77. `// });`  
   Ends the commented-out configuration.

78. blank line  
   Keeps the service registration area organized.

79. `builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();`  
   Registers email verification logic.

80. `builder.Services.AddScoped<INotificationService, NotificationService>();`  
   Registers notification service for user alerts.

81. `builder.Services.AddScoped<IMyDeenService, MyDeenService>();`  
   Registers the My Deen tracker functionality.

82. `builder.Services.AddHttpClient<IHadithService, HadithService>();`  
   Registers the Hadith service with the default HTTP client factory configuration.

83. `builder.Services.AddHostedService<HadithSchedulerService>();`  
   Starts a background service that refreshes or rotates daily Hadith data.

84. `builder.Services.AddScoped<IPhotoService, PhotoService>();`  
   Registers media upload and storage logic.

85. `builder.Services.AddScoped<ISslCommerzService, SslCommerzService>();`  
   Registers payment integration logic.

86. `builder.Services.AddSingleton<IAboutService, AboutService>();`  
   Registers the static about-page service as a singleton because it is mostly read-only.

87. blank line  
   Prepares to build the app instance.

88. `var app = builder.Build();`  
   Builds the ASP.NET Core app with all registered services and configuration.

89. blank line  
   Keeps the startup initialization readable.

90. `// Initialization is deliberately performed before the request pipeline starts`  
   Comments why database seeding happens before beginning HTTP request handling.

91. `// so roles, migrations, and baseline data are available to the first request.`  
   Exposes the reason for pre-start initialization.

92. `try`  
   Begins a guarded database initialization block.

93. `{`  
   Opens the try block.

94. `    await DbInitializer.SeedDatabaseAsync(app.Services);`  
   Populates required roles, default data, and initial system records before serving requests.

95. `}`  
   Closes the try block.

96. `catch (Exception ex)`  
   Catches any seeding failure.

97. `{`  
   Opens the catch block.

98. `    var logger = app.Services.GetRequiredService<ILogger<Program>>();`  
   Gets a logger for the app startup process.

99. `    logger.LogError(ex, "An error occurred during database seeding.");`  
   Writes the failure to logs for debugging.

100. `}`  
   Closes the catch block.

101. blank line  
   Gap before middleware configuration.

102. `// Configure middleware before mapping routes and the SignalR hub.`  
   Documents the order of middleware registration.

103. `if (!app.Environment.IsDevelopment())`  
   Enables production-only middleware in non-development environments.

104. `{`  
   Opens the environment check block.

105. `    app.UseExceptionHandler("/Home/Error");`  
   Redirects unhandled exceptions to the error page.

106. `    app.UseHsts();`  
   Enables HTTP Strict Transport Security in production.

107. `}`  
   Closes the conditional block.

108. blank line  
   Break before pipeline setup.

109. `app.UseHttpsRedirection();`  
   Forces HTTP traffic to HTTPS for secure communication.

110. `app.UseStaticFiles();`  
   Serves static assets like CSS, JS, and images.

111. blank line  
   Keeps routing separate.

112. `app.UseRouting();`  
   Matches incoming requests to endpoint routes.

113. blank line  
   Prepares authentication middleware.

114. `app.UseAuthentication();`  
   Validates the current user identity from the authentication cookie.

115. `app.UseAuthorization();`  
   Enforces permission checks for protected actions.

116. blank line  
   Marks the route mapping section.

117. `// These aliases preserve the public, user-facing URLs while controllers retain`  
   Explains why the app uses route aliases instead of exposing raw controller names.

118. `// conventional action names internally.`  
   Clarifies the model behind the route mapping design.

119. `app.MapControllerRoute(name: "about", pattern: "about", defaults: new { controller = "Home", action = "About" });`  
   Maps the /about URL to the About action to keep public URLs simple.

120. `app.MapControllerRoute(name: "contact", pattern: "contact", defaults: new { controller = "Messages", action = "Index" });`  
   Maps contact page URL.

121. `app.MapControllerRoute(name: "sdg9", pattern: "sdg9", defaults: new { controller = "Home", action = "SDG9" });`  
   Maps a custom route for the SDG9 page.

122. `app.MapControllerRoute(name: "privacy", pattern: "privacy", defaults: new { controller = "Home", action = "Privacy" });`  
   Maps privacy policy route.

123. blank line  
   Starts user-area route definitions.

124. `app.MapControllerRoute(name: "mydeen", pattern: "my-deen", defaults: new { controller = "MyDeen", action = "Index" });`  
   Maps My Deen route.

125. `app.MapControllerRoute(name: "notifications", pattern: "notifications", defaults: new { controller = "Notifications", action = "Index" });`  
   Maps notifications route.

126. `app.MapControllerRoute(name: "calendar", pattern: "islamic-calendar", defaults: new { controller = "IslamicCalendar", action = "Index" });`  
   Maps Islamic calendar route.

127. blank line  
   Continues route declarations.

128. `app.MapControllerRoute(name: "prayertimes", pattern: "prayer-times", defaults: new { controller = "PrayerTimes", action = "Index" });`  
   Maps prayer times route.

129. `app.MapControllerRoute(name: "events", pattern: "events", defaults: new { controller = "IslamicCalendar", action = "Index" });`  
   Maps event routes.

130. `app.MapControllerRoute(name: "activitiesPrograms", pattern: "activities-and-programs", defaults: new { controller = "Activities", action = "Index" });`  
   Maps the activities page URL.

131. `app.MapControllerRoute(name: "activities", pattern: "activities", defaults: new { controller = "Activities", action = "Index" });`  
   Maps the shorter activities route.

132. `app.MapControllerRoute(name: "activityDetails", pattern: "activities/{id:int}", defaults: new { controller = "Activities", action = "Details" });`  
   Maps activity detail URLs using an integer id parameter.

133. `app.MapControllerRoute(name: "myActivities", pattern: "my-activities", defaults: new { controller = "Activities", action = "MyActivities" });`  
   Maps the user-specific activities page.

134. blank line  
   Continues donation and program route mappings.

135. `app.MapControllerRoute(name: "zakat", pattern: "zakat", defaults: new { controller = "Zakat", action = "Index" });`  
   Maps zakat page route.

136. `app.MapControllerRoute(name: "donate", pattern: "donate", defaults: new { controller = "Donate", action = "Index" });`  
   Maps donation route.

137. `app.MapControllerRoute(name: "zakatDonate", pattern: "zakat-and-donate", defaults: new { controller = "Zakat", action = "Index" });`  
   Maps a combined zakat/donate route.

138. `app.MapControllerRoute(name: "donateSuccess", pattern: "donate/success", defaults: new { controller = "Donate", action = "Success" });`  
   Maps the donation success page.

139. `app.MapControllerRoute(name: "myDonations", pattern: "my-donations", defaults: new { controller = "Donate", action = "MyDonations" });`  
   Maps user donation history route.

140. blank line  
   Continues event and programs mapping.

141. `app.MapControllerRoute(name: "milad", pattern: "milad", defaults: new { controller = "Activities", action = "Index" });`  
   Maps Milad route.

142. `app.MapControllerRoute(name: "myMilads", pattern: "my-milad-requests", defaults: new { controller = "Activities", action = "MyActivities" });`  
   Maps the user’s Milad request list.

143. blank line  
   Sets up messaging and AI routes.

144. `app.MapControllerRoute(name: "messaging", pattern: "messaging", defaults: new { controller = "Messages", action = "Index" });`  
   Maps the messaging screen route.

145. `app.MapControllerRoute(name: "diinai", pattern: "diin-ai", defaults: new { controller = "DiinAI", action = "Index" });`  
   Maps the Diin AI page route.

146. `app.MapControllerRoute(name: "productAnalyzer", pattern: "product-analyzer", defaults: new { controller = "ProductAnalyzer", action = "Index" });`  
   Maps the product analyzer page route.

147. blank line  
   Starts authentication and account routes.

148. `app.MapControllerRoute(name: "userLogin", pattern: "user-login", defaults: new { controller = "Account", action = "Login" });`  
   Maps login route.

149. `app.MapControllerRoute(name: "userRegistration", pattern: "user-registration", defaults: new { controller = "Account", action = "Register" });`  
   Maps registration route.

150. `app.MapControllerRoute(name: "userProfile", pattern: "user-profile", defaults: new { controller = "Account", action = "Profile" });`  
   Maps profile route.

151. `app.MapControllerRoute(name: "verifyEmail", pattern: "verify-email", defaults: new { controller = "Account", action = "VerifyEmail" });`  
   Maps email verification route.

152. blank line  
   Continues admin routes.

153. `app.MapControllerRoute(name: "adminRegistrations", pattern: "admin/registrations", defaults: new { controller = "Admin", action = "Registrations" });`  
   Maps admin registration management route.

154. `app.MapControllerRoute(name: "adminPrograms", pattern: "admin/programs", defaults: new { controller = "Admin", action = "Activities" });`  
   Maps admin program management route.

155. `app.MapControllerRoute(name: "adminMessages", pattern: "admin/messages", defaults: new { controller = "Admin", action = "Messages" });`  
   Maps admin messages route.

156. `app.MapControllerRoute(name: "adminPanel", pattern: "admin/panel", defaults: new { controller = "Admin", action = "Dashboard" });`  
   Maps admin panel route.

157. `app.MapControllerRoute(name: "adminDashboard", pattern: "admin-dashboard", defaults: new { controller = "Admin", action = "Dashboard" });`  
   Maps a secondary admin dashboard route alias.

158. blank line  
   Setup for real-time communication hub.

159. `app.MapHub<SupportChatHub>("/hubs/support-chat");`  
   Exposes the SignalR support chat endpoint for real-time chats.

160. blank line  
   Prepares the default fallback route.

161. `// Default Conventional Route`  
   Marks the generic route used by the app when no custom route matches.

162. `app.MapControllerRoute(`  
   Starts the default controller route definition.

163. `    name: "default",`  
   Names the route as default.

164. `    pattern: "{controller=Home}/{action=Index}/{id?}");`  
   Maps the default path to Home/Index, with optional id param.

165. blank line  
   Final line before app startup.

166. `app.Run();`  
   Starts the application and listens for HTTP requests.

---

## 2) AdDiin/Data/ApplicationDbContext.cs

### Line-by-line explanation

1. `using AdDiin.Models.Entities;`  
   Imports the domain entities used by the database model.

2. `using Microsoft.AspNetCore.Identity;`  
   Imports Identity user and role primitives.

3. `using Microsoft.AspNetCore.Identity.EntityFrameworkCore;`  
   Imports the base Identity EF Core class that the app uses for user management.

4. `using Microsoft.EntityFrameworkCore;`  
   Imports EF Core APIs for modeling and database configuration.

5. blank line  
   Keeps the namespace section clean.

6. `namespace AdDiin.Data`  
   Declares the data layer namespace.

7. `{`  
   Opens the namespace.

8. `    /// <summary>`  
   Starts XML summary comment.

9. `    /// EF Core database context for Identity data and the platform's domain entities.`  
   Describes the purpose of the DbContext class.

10. `    /// </summary>`  
   Closes the summary comment.

11. `    public class ApplicationDbContext`  
   Defines the EF Core data context.

12. `        : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>`  
   Inherits from the standard ASP.NET Core Identity DbContext with user and role types.

13. `{`  
   Opens the class body.

14. `        /// <summary>`  
   Starts constructor summary comment.

15. `        /// Creates the context with the provider options configured by the host.`  
   Explains that the database options are injected by ASP.NET Core.

16. `        /// </summary>`  
   Closes the constructor summary comment.

17. `        public ApplicationDbContext(`  
   Declares the constructor.

18. `            DbContextOptions<ApplicationDbContext> options)`  
   Accepts EF Core database options from DI.

19. `            : base(options)`  
   Calls the base IdentityDbContext constructor with the configured options.

20. `        {`  
   Opens constructor body.

21. `        }`  
   Closes the constructor.

22. blank line  
   Break before entity sets.

23. `        public DbSet<PrayerTime> PrayerTimes => Set<PrayerTime>();`  
   Exposes the PrayerTime table in the EF model.

24. `        public DbSet<IslamicEvent> IslamicEvents => Set<IslamicEvent>();`  
   Exposes Islamic events table.

25. `        public DbSet<MiladRequest> MiladRequests => Set<MiladRequest>();`  
   Exposes Milad requests table.

26. `        public DbSet<Donation> Donations => Set<Donation>();`  
   Exposes donations table.

27. `        public DbSet<Conversation> Conversations => Set<Conversation>();`  
   Exposes support conversations table.

28. `        public DbSet<Message> Messages => Set<Message>();`  
   Exposes chat messages table.

29. `        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();`  
   Exposes contact form messages.

30. `        public DbSet<Activity> Activities => Set<Activity>();`  
   Exposes activities/programs table.

31. `        public DbSet<ProgramRegistration> ProgramRegistrations => Set<ProgramRegistration>();`  
   Exposes registrations to activities and programs.

32. `        public DbSet<DhikrRecord> DhikrRecords => Set<DhikrRecord>();`  
   Exposes daily Dhikr records.

33. `        public DbSet<QuranReadingLog> QuranReadingLogs => Set<QuranReadingLog>();`  
   Exposes Quran reading logs.

34. `        public DbSet<AdhkarLog> AdhkarLogs => Set<AdhkarLog>();`  
   Exposes adhkar/Islamic reminders log.

35. `        public DbSet<RuqyahLog> RuqyahLogs => Set<RuqyahLog>();`  
   Exposes Ruqyah logs.

36. `        public DbSet<DailyDeenGoal> DailyDeenGoals => Set<DailyDeenGoal>();`  
   Exposes daily deen goal tracking table.

37. `        public DbSet<UserNotification> UserNotifications => Set<UserNotification>();`  
   Exposes notifications table.

38. `        public DbSet<UserDeenSettings> UserDeenSettings => Set<UserDeenSettings>();`  
   Exposes user My Deen settings table.

39. `        public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();`  
   Exposes email verification token table.

40. `        public DbSet<DiinAIConversation> DiinAIConversations => Set<DiinAIConversation>();`  
   Exposes AI conversation table.

41. `        public DbSet<DiinAIMessage> DiinAIMessages => Set<DiinAIMessage>();`  
   Exposes AI message history table.

42. blank line  
   Creates spacing before optional feature entries.

43. `        // Feature branch code — kept for reference only.`  
   Notes the next class property is commented out as historical code.

44. `        // It is intentionally inactive.`  
   Clarifies it is disabled intentionally.

45. `        // public DbSet<ScheduledHadith> ScheduledHadiths => Set<ScheduledHadith>();`  
   Example of a feature branch entity that is deliberately left inactive.

46. blank line  
   Before model configuration method.

47. `        /// <summary>`  
   Starts summary comment for model configuration.

48. `        /// Configures relationship delete behavior and indexes that are not`  
   Explains the reason for the custom configuration.

49. `        /// expressible through entity annotations alone.`  
   Clarifies that some constraints are easier to set here in the model builder.

50. `        /// </summary>`  
   Ends the summary.

51. `        protected override void OnModelCreating(ModelBuilder builder)`  
   Overrides the EF Core model builder method to define custom schema rules.

52. `{`  
   Opens the method body.

53. `            base.OnModelCreating(builder);`  
   Calls the base Identity model configuration first.

54. blank line  
   Break before conversation relationships.

55. `            // Configure Conversation relationships with NoAction/Restrict to prevent SQL Server cascade cycle`  
   Explains why conversation relationships use Restrict instead of cascade delete.

56. `            builder.Entity<Conversation>()`  
   Begins configuring conversation entity.

57. `                .HasOne(c => c.User)`  
   Sets the user relationship.

58. `                .WithMany(u => u.UserConversations)`  
   Maps the reverse navigation property on the user.

59. `                .HasForeignKey(c => c.UserId)`  
   Uses UserId as the FK.

60. `                .OnDelete(DeleteBehavior.Restrict);`  
   Prevents automatic cascade deletion to avoid cycles.

61. blank line  
   Starts admin conversation mapping.

62. `            builder.Entity<Conversation>()`  
   Configures another Conversation relationship.

63. `                .HasOne(c => c.Admin)`  
   Sets admin relation.

64. `                .WithMany(u => u.AdminConversations)`  
   Maps reverse admin side collection.

65. `                .HasForeignKey(c => c.AdminId)`  
   Uses AdminId as the foreign key.

66. `                .OnDelete(DeleteBehavior.Restrict);`  
   Prevents delete cycles.

67. blank line  
   Break between relationship groups.

68. `            // Configure Message relationships`  
   Starts message relationship config block.

69. `            builder.Entity<Message>()`  
   Begins Message config.

70. `                .HasOne(m => m.Conversation)`  
   Maps to the related conversation.

71. `                .WithMany(c => c.Messages)`  
   Maps the conversation’s collection of messages.

72. `                .HasForeignKey(m => m.ConversationId)`  
   Uses ConversationId as foreign key.

73. `                .OnDelete(DeleteBehavior.Cascade);`  
   Deletes messages when their conversation is removed.

74. blank line  
   Starts sender relation config.

75. `            builder.Entity<Message>()`  
   Configures Message sender relation.

76. `                .HasOne(m => m.Sender)`  
   Maps the message sender user.

77. `                .WithMany(u => u.Messages)`  
   Maps the user’s message list.

78. `                .HasForeignKey(m => m.SenderId)`  
   Uses SenderId as foreign key.

79. `                .OnDelete(DeleteBehavior.Restrict);`  
   Prevents deleting users who still have messages.

80. blank line  
   Continues to program registration config.

81. `            // Configure ProgramRegistration relationship`  
   Notes the next relationship setup.

82. `            builder.Entity<ProgramRegistration>()`  
   Starts ProgramRegistration config.

83. `                .HasOne(r => r.Activity)`  
   Maps the linked activity.

84. `                .WithMany(a => a.Registrations)`  
   Maps the list of registrations for the activity.

85. `                .HasForeignKey(r => r.ActivityId)`  
   Uses ActivityId as FK.

86. `                .OnDelete(DeleteBehavior.Cascade);`  
   Deletes registrations when the activity is removed.

87. blank line  
   Starts user relation for registration.

88. `            builder.Entity<ProgramRegistration>()`  
   Begins second registration relationship.

89. `                .HasOne(r => r.User)`  
   Maps the user that registered.

90. `                .WithMany()`  
   Uses no collection on the user entity.

91. `                .HasForeignKey(r => r.UserId)`  
   Uses UserId as FK.

92. `                .OnDelete(DeleteBehavior.SetNull);`  
   Makes registration user optional when user is deleted.

93. blank line  
   Starts index creation.

94. `            builder.Entity<ProgramRegistration>()`  
   Configures registration index.

95. `                .HasIndex(r => r.Status);`  
   Creates an index for filtering registrations by status.

96. blank line  
   Continues to MiladRequest config.

97. `            // Configure MiladRequest relationship`  
   Explanation for the next relationship block.

98. `            builder.Entity<MiladRequest>()`  
   Begins MiladRequest config.

99. `                .HasOne(m => m.User)`  
   Maps the user relation.

100. `                .WithMany(u => u.MiladRequests)`  
   Maps user’s Milad requests.

101. `                .HasForeignKey(m => m.UserId)`  
   Sets UserId as foreign key.

102. `                .OnDelete(DeleteBehavior.Restrict);`  
   Prevents deleting users who have open Milad requests.

103. blank line  
   Starts index for Milad status.

104. `            builder.Entity<MiladRequest>()`  
   Prepares MiladRequest index.

105. `                .HasIndex(m => m.Status);`  
   Indexes request status for filtering.

106. blank line  
   Continues donation configuration.

107. `            // Configure Donation relationship`  
   Start donation config.

108. `            builder.Entity<Donation>()`  
   Begins Donation config.

109. `                .HasOne(d => d.User)`  
   Maps donation user relation.

110. `                .WithMany(u => u.Donations)`  
   Maps the user’s donations list.

111. `                .HasForeignKey(d => d.UserId)`  
   Uses UserId.

112. `                .OnDelete(DeleteBehavior.SetNull);`  
   Keeps donation records if the user is removed.

113. blank line  
   Starts indices for donation record lookup.

114. `            builder.Entity<Donation>()`  
   Begins donation index config.

115. `                .HasIndex(d => d.TranId)`  
   Creates unique index for transaction ID.

116. `                .IsUnique();`  
   Makes transaction IDs unique.

117. blank line  
   Continues additional donation indexes.

118. `            builder.Entity<Donation>()`  
   Configures another donation index.

119. `                .HasIndex(d => d.PaymentStatus);`  
   Creates index for payment status queries.

120. blank line  
   Another donation index.

121. `            builder.Entity<Donation>()`  
   Starts category index config.

122. `                .HasIndex(d => d.Category);`  
   Creates index for filtering by donation category.

123. blank line  
   Begins prayer time model config.

124. `            // Configure PrayerTime`  
   Start prayer timetable config.

125. `            builder.Entity<PrayerTime>()`  
   Begins PrayerTime setup.

126. `                .HasIndex(p => p.PrayerName)`  
   Creates index on prayer name.

127. `                .IsUnique();`  
   Ensures a prayer name appears once.

128. blank line  
   Prayer display ordering.

129. `            builder.Entity<PrayerTime>()`  
   Continues PrayerTime config.

130. `                .HasIndex(p => p.DisplayOrder);`  
   Indexes display ordering for UI sorting.

131. blank line  
   Verification code table configuration.

132. `            // Configure VerificationCode`  
   Marks verification code setup.

133. `            builder.Entity<VerificationCode>()`  
   Begins verification code config.

134. `                .HasIndex(v => v.Email);`  
   Creates index on email for lookup speed.

135. blank line  
   Begins Diin AI conversation relationships.

136. `            builder.Entity<DiinAIConversation>()`  
   Begins AI conversation config.

137. `                .HasOne(c => c.User)`  
   Maps the user who owns the AI conversation.

138. `                .WithMany()`  
   No reverse collection on User.

139. `                .HasForeignKey(c => c.UserId)`  
   Uses UserId as FK.

140. `                .OnDelete(DeleteBehavior.Cascade);`  
   Deletes AI conversations when the user is removed.

141. blank line  
   Another AI-related config.

142. `            builder.Entity<DiinAIMessage>()`  
   Begins AI message config.

143. `                .HasOne(m => m.Conversation)`  
   Maps message’s conversation.

144. `                .WithMany(c => c.Messages)`  
   Maps reverse message history list.

145. `                .HasForeignKey(m => m.ConversationId)`  
   Uses ConversationId as FK.

146. `                .OnDelete(DeleteBehavior.Cascade);`  
   Deletes messages with the conversation.

147. blank line  
   Starts AI conversation composite index.

148. `            builder.Entity<DiinAIConversation>()`  
   Begins AI conversation index config.

149. `                .HasIndex(c => new { c.UserId, c.UpdatedAt });`  
   Indexes by user and update time for recent conversation lookup.

150. blank line  
   Feature branch section.

151. `            // Feature branch configuration — kept for reference only.`  
   Describes inactive feature code.

152. `            // It is intentionally inactive.`  
   Clarifies it is not active.

153. `            // builder.Entity<ScheduledHadith>()`  
   Shows old feature entity configuration.

154. `            //     .HasIndex(h => new { h.SlotDate, h.SlotTime })`  
   Defines unique scheduled hadith index.

155. `            //     .IsUnique();`  
   Ensures no duplicate scheduled hadith slot is created.

156. `        }`  
   Closes the OnModelCreating method.

157. `    }`  
   Closes the class.

158. `}`  
   Closes the namespace.

---

---

## 3) AdDiin/AdDiin.csproj

### Line-by-line explanation

1. `<Project Sdk="Microsoft.NET.Sdk.Web">`  
   Declares an ASP.NET Core web project and enables the web SDK defaults.

2. `<PropertyGroup>`  
   Starts the project-wide build and compiler settings.

3. `<TargetFramework>net9.0</TargetFramework>`  
   Targets the .NET 9 runtime and APIs.

4. `<Nullable>enable</Nullable>`  
   Enables nullable reference type analysis to catch possible null values during compilation.

5. `<ImplicitUsings>enable</ImplicitUsings>`  
   Automatically imports common .NET namespaces so individual source files need fewer `using` statements.

6. `<UserSecretsId>9b0549b7-fb06-4c46-ac91-c87e103e0021</UserSecretsId>`  
   Identifies the local user-secrets store used for development-only configuration.

7. `</PropertyGroup>`  
   Ends the project property settings.

8. `<ItemGroup>`  
   Starts the NuGet package reference list.

9. `<PackageReference Include="CloudinaryDotNet" Version="1.29.3" />`  
   Adds Cloudinary support for image or media storage.

10. `<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.2" />`  
   Adds the SQL Server provider for Entity Framework Core.

11. `<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.2">`  
   Adds EF Core command-line and migration tooling.

12. `<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>`  
   Specifies which package assets remain available to the project and its build process.

13. `<PrivateAssets>all</PrivateAssets>`  
   Prevents the tooling package from flowing to projects that reference this project.

14. `</PackageReference>`  
   Ends the EF Core Tools package configuration.

15. `<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.2">`  
   Adds design-time EF Core support used for migrations and database scaffolding.

16. `<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>`  
   Defines the design package assets available during development and build operations.

17. `<PrivateAssets>all</PrivateAssets>`  
   Keeps the design-time package private to this project.

18. `</PackageReference>`  
   Ends the EF Core Design package configuration.

19. `<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.2" />`  
   Adds EF Core stores and integration for ASP.NET Core Identity users and roles.

20. `</ItemGroup>`  
   Ends the NuGet package list.

21. `</Project>`  
   Ends the project definition.

The blank lines in this file are formatting separators; they do not change runtime behavior.

## Suggested use in Git

Use this file as a checkpoint document for reviewable commits:

- commit 1: project startup + dependency registration
- commit 2: database setup + identity
- commit 3: route definitions and middleware
- commit 4: My Deen and daily tracking setup
- commit 5: AI and notification services

Before each commit, use the matching section above as the commit explanation. Keep
one logical feature per commit, then verify the result with `dotnet build` and
`git diff --check`. This keeps the history easy to review without placing
explanatory comments inside production code.

This makes it easier to explain what changed in each commit without hiding the code flow.

