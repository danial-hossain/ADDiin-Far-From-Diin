using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=AdDiinDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie Settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/user-login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Add MVC Services
builder.Services.AddControllersWithViews();

// Register Application Services for Dependency Injection
builder.Services.AddScoped<IPrayerTimeService, PrayerTimeService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IMiladService, MiladService>();
builder.Services.AddScoped<IIslamicEventService, IslamicEventService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddHttpClient<IDiinAIService, DiinAIService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var timeoutSeconds = config.GetValue<int>("AISettings:TimeoutSeconds", 120);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddHttpClient<IHalalDetectorService, HalalDetectorService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var timeoutSeconds = config.GetValue<int>("AISettings:TimeoutSeconds", 120);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMyDeenService, MyDeenService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ISslCommerzService, SslCommerzService>();
builder.Services.AddSingleton<IAboutService, AboutService>();

var app = builder.Build();

// Seed Database automatically on startup
try
{
    await DbInitializer.SeedDatabaseAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database seeding.");
}

// Configure HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route Aliases matching user-centric structure
app.MapControllerRoute(name: "about", pattern: "about", defaults: new { controller = "Home", action = "About" });
app.MapControllerRoute(name: "contact", pattern: "contact", defaults: new { controller = "Home", action = "Contact" });
app.MapControllerRoute(name: "sdg9", pattern: "sdg9", defaults: new { controller = "Home", action = "SDG9" });
app.MapControllerRoute(name: "privacy", pattern: "privacy", defaults: new { controller = "Home", action = "Privacy" });

app.MapControllerRoute(name: "mydeen", pattern: "my-deen", defaults: new { controller = "MyDeen", action = "Index" });
app.MapControllerRoute(name: "notifications", pattern: "notifications", defaults: new { controller = "Notifications", action = "Index" });
app.MapControllerRoute(name: "calendar", pattern: "islamic-calendar", defaults: new { controller = "IslamicCalendar", action = "Index" });

app.MapControllerRoute(name: "prayertimes", pattern: "prayer-times", defaults: new { controller = "PrayerTimes", action = "Index" });
app.MapControllerRoute(name: "events", pattern: "events", defaults: new { controller = "IslamicCalendar", action = "Index" });
app.MapControllerRoute(name: "activitiesPrograms", pattern: "activities-and-programs", defaults: new { controller = "Activities", action = "Index" });
app.MapControllerRoute(name: "activities", pattern: "activities", defaults: new { controller = "Activities", action = "Index" });
app.MapControllerRoute(name: "activityDetails", pattern: "activities/{id:int}", defaults: new { controller = "Activities", action = "Details" });
app.MapControllerRoute(name: "myActivities", pattern: "my-activities", defaults: new { controller = "Activities", action = "MyActivities" });

app.MapControllerRoute(name: "zakat", pattern: "zakat", defaults: new { controller = "Zakat", action = "Index" });
app.MapControllerRoute(name: "donate", pattern: "donate", defaults: new { controller = "Donate", action = "Index" });
app.MapControllerRoute(name: "zakatDonate", pattern: "zakat-and-donate", defaults: new { controller = "Zakat", action = "Index" });
app.MapControllerRoute(name: "donateSuccess", pattern: "donate/success", defaults: new { controller = "Donate", action = "Success" });
app.MapControllerRoute(name: "myDonations", pattern: "my-donations", defaults: new { controller = "Donate", action = "MyDonations" });

app.MapControllerRoute(name: "milad", pattern: "milad", defaults: new { controller = "Activities", action = "Index" });
app.MapControllerRoute(name: "myMilads", pattern: "my-milad-requests", defaults: new { controller = "Activities", action = "MyActivities" });

app.MapControllerRoute(name: "messaging", pattern: "messaging", defaults: new { controller = "Messages", action = "Index" });
app.MapControllerRoute(name: "diinai", pattern: "diin-ai", defaults: new { controller = "DiinAI", action = "Index" });
app.MapControllerRoute(name: "productAnalyzer", pattern: "product-analyzer", defaults: new { controller = "ProductAnalyzer", action = "Index" });

app.MapControllerRoute(name: "userLogin", pattern: "user-login", defaults: new { controller = "Account", action = "Login" });
app.MapControllerRoute(name: "userRegistration", pattern: "user-registration", defaults: new { controller = "Account", action = "Register" });
app.MapControllerRoute(name: "userProfile", pattern: "user-profile", defaults: new { controller = "Account", action = "Profile" });
app.MapControllerRoute(name: "verifyEmail", pattern: "verify-email", defaults: new { controller = "Account", action = "VerifyEmail" });

app.MapControllerRoute(name: "adminRegistrations", pattern: "admin/registrations", defaults: new { controller = "Admin", action = "Registrations" });
app.MapControllerRoute(name: "adminPrograms", pattern: "admin/programs", defaults: new { controller = "Admin", action = "Activities" });
app.MapControllerRoute(name: "adminPanel", pattern: "admin/panel", defaults: new { controller = "Admin", action = "Dashboard" });
app.MapControllerRoute(name: "adminDashboard", pattern: "admin-dashboard", defaults: new { controller = "Admin", action = "Dashboard" });

// Default Conventional Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();