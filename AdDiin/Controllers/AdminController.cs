using AdDiin.Data;
using AdDiin.Models.Entities;
using AdDiin.Models.ViewModels;
using AdDiin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdDiin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPrayerTimeService _prayerService;
        private readonly IIslamicEventService _eventService;
        private readonly IDonationService _donationService;
        private readonly IMiladService _miladService;
        private readonly IActivityService _activityService;
        private readonly IContactService _contactService;
        private readonly IMessagingService _messagingService;
        private readonly IAboutService _aboutService;
        private readonly IPhotoService _photoService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IPrayerTimeService prayerService,
            IIslamicEventService eventService,
            IDonationService donationService,
            IMiladService miladService,
            IActivityService activityService,
            IContactService contactService,
            IMessagingService messagingService,
            IAboutService aboutService,
            IPhotoService photoService)
        {
            _context = context;
            _userManager = userManager;
            _prayerService = prayerService;
            _eventService = eventService;
            _donationService = donationService;
            _miladService = miladService;
            _activityService = activityService;
            _contactService = contactService;
            _messagingService = messagingService;
            _aboutService = aboutService;
            _photoService = photoService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            var adminUsers = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            var regularUsers = totalUsers - adminUsers;

            var totalCompletedDonations = await _context.Donations
                .Where(d => d.PaymentStatus == "completed")
                .SumAsync(d => (decimal?)d.Amount) ?? 0;

            var totalDonationsCount = await _context.Donations.CountAsync();
            var pendingDonationsCount = await _context.Donations.CountAsync(d => d.PaymentStatus == "pending");

            var pendingRegistrations = await _context.ProgramRegistrations.CountAsync(r => r.Status == "Pending");
            var totalRegistrations = await _context.ProgramRegistrations.CountAsync();

            var pendingMilads = await _context.MiladRequests.CountAsync(m => m.Status == "pending");
            var totalMilads = await _context.MiladRequests.CountAsync();

            var totalEvents = await _context.IslamicEvents.CountAsync();
            var totalActivities = await _context.Activities.CountAsync();
            var unreadContact = await _context.ContactMessages.CountAsync(c => c.Status == "unread");
            var activeConversations = await _context.Conversations.CountAsync(c => c.Status == "active");

            var recentDonations = await _context.Donations
                .Include(d => d.User)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentRegistrations = await _context.ProgramRegistrations
                .Include(r => r.Activity)
                .Include(r => r.User)
                .OrderByDescending(r => r.RegisteredAt)
                .Take(5)
                .ToListAsync();

            var recentMilads = await _context.MiladRequests
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            var upcomingEvents = await _context.IslamicEvents
                .Where(e => e.IsActive && e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Take(4)
                .ToListAsync();

            var activePrograms = await _context.Activities
                .Where(a => a.IsActive)
                .OrderBy(a => a.ProgramDate)
                .Take(4)
                .ToListAsync();

            var categoryBreakdown = await _donationService.GetCategoryBreakdownAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                AdminCount = adminUsers,
                RegularUserCount = regularUsers,
                TotalDonationsCompleted = totalCompletedDonations,
                TotalDonationsCount = totalDonationsCount,
                PendingDonationsCount = pendingDonationsCount,
                PendingRegistrationsCount = pendingRegistrations,
                TotalRegistrationsCount = totalRegistrations,
                PendingMiladCount = pendingMilads,
                TotalMiladCount = totalMilads,
                TotalEventsCount = totalEvents,
                TotalActivitiesCount = totalActivities,
                UnreadContactCount = unreadContact,
                ActiveConversationsCount = activeConversations,
                RecentDonations = recentDonations,
                RecentRegistrations = recentRegistrations,
                RecentMiladRequests = recentMilads,
                UpcomingEvents = upcomingEvents,
                ActivePrograms = activePrograms,
                DonationsByCategory = categoryBreakdown
            };

            return View(vm);
        }

        // ================= USERS =================
        public async Task<IActionResult> Users(string? search, string? role, string? status)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.FullName.Contains(s) || (u.Email != null && u.Email.Contains(s)) || (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                var isActive = status == "active";
                query = query.Where(u => u.IsActive == isActive);
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            var vm = new AdminUsersViewModel
            {
                Users = users,
                SearchQuery = search,
                RoleFilter = role,
                StatusFilter = status
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUser(int id)
        {
            var currentUserId = (await _userManager.GetUserAsync(User))?.Id;
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"User status updated to {(user.IsActive ? "Active" : "Inactive")}.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(int id, string newRole)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Prevent demoting last admin
            if (currentRoles.Contains("Admin") && newRole != "Admin")
            {
                var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                if (adminCount <= 1)
                {
                    TempData["ErrorMessage"] = "Cannot remove the last administrator.";
                    return RedirectToAction(nameof(Users));
                }
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["SuccessMessage"] = $"User role updated to {newRole}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = (await _userManager.GetUserAsync(User))?.Id;
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                if (adminCount <= 1)
                {
                    TempData["ErrorMessage"] = "Cannot delete the last administrator.";
                    return RedirectToAction(nameof(Users));
                }
            }

            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction(nameof(Users));
        }

        // ================= PRAYER TIMES =================
        public async Task<IActionResult> PrayerTimes()
        {
            var prayers = await _prayerService.GetAllAsync();
            return View(prayers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrayerTimeCreate(PrayerTime model)
        {
            if (ModelState.IsValid)
            {
                await _prayerService.CreateAsync(model);
                TempData["SuccessMessage"] = "Prayer time created successfully.";
            }
            return RedirectToAction(nameof(PrayerTimes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrayerTimeEdit(int id, PrayerTime model)
        {
            if (ModelState.IsValid)
            {
                await _prayerService.UpdateAsync(id, model);
                TempData["SuccessMessage"] = "Prayer time updated successfully.";
            }
            return RedirectToAction(nameof(PrayerTimes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrayerTimeToggle(int id)
        {
            await _prayerService.ToggleActiveAsync(id);
            return RedirectToAction(nameof(PrayerTimes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrayerTimeDelete(int id)
        {
            await _prayerService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Prayer time deleted.";
            return RedirectToAction(nameof(PrayerTimes));
        }

        // ================= EVENTS =================
        public async Task<IActionResult> Events()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventCreate(IslamicEvent model)
        {
            if (ModelState.IsValid)
            {
                await _eventService.CreateAsync(model);
                TempData["SuccessMessage"] = "Islamic event created successfully.";
            }
            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventEdit(int id, IslamicEvent model)
        {
            if (ModelState.IsValid)
            {
                await _eventService.UpdateAsync(id, model);
                TempData["SuccessMessage"] = "Islamic event updated successfully.";
            }
            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventDelete(int id)
        {
            await _eventService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Events));
        }

        // ================= MILADS =================
        public async Task<IActionResult> Milads(string? status, string? search)
        {
            var list = await _miladService.GetAdminListAsync(status, search);
            var vm = new AdminMiladsViewModel
            {
                MiladRequests = list,
                StatusFilter = status,
                SearchQuery = search
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MiladUpdateStatus(int id, string status, string? adminRemark)
        {
            await _miladService.UpdateStatusAsync(id, status, adminRemark);
            TempData["SuccessMessage"] = $"Milad status updated to '{status}'.";
            return RedirectToAction(nameof(Milads));
        }

        // ================= DONATIONS =================
        public async Task<IActionResult> Donations(string? category, string? status, string? search)
        {
            var vm = await _donationService.GetAdminDonationsAsync(category, status, search);
            return View(vm);
        }

        // ================= ACTIVITIES =================
        public async Task<IActionResult> Activities(string? search, string? category)
        {
            var activities = await _activityService.GetAllActivitiesAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                activities = activities.Where(a => a.Title.ToLower().Contains(s) || 
                                                  a.Description.ToLower().Contains(s) || 
                                                  (a.Location != null && a.Location.ToLower().Contains(s)) ||
                                                  (a.Organizer != null && a.Organizer.ToLower().Contains(s))).ToList();
            }

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                activities = activities.Where(a => a.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.Search = search;
            ViewBag.Category = category;
            return View(activities);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivityCreate(Activity model, IFormFile? imageFile)
        {
            ModelState.Remove(nameof(model.Registrations));

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadRes = await _photoService.AddPhotoAsync(imageFile, "ad-diin/activities");
                        if (uploadRes != null && uploadRes.SecureUrl != null)
                        {
                            model.ImageUrl = uploadRes.SecureUrl.ToString();
                            model.ImagePublicId = uploadRes.PublicId;
                        }
                        else if (uploadRes?.Error != null)
                        {
                            TempData["ErrorMessage"] = $"Cloudinary upload warning: {uploadRes.Error.Message}";
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Cloudinary upload error: {ex.Message}";
                    }
                }

                await _activityService.CreateAsync(model);
                TempData["SuccessMessage"] = "Activity created successfully!";
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation error: {errors}";
            }
            return RedirectToAction(nameof(Activities));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivityEdit(int id, Activity model, IFormFile? imageFile)
        {
            ModelState.Remove(nameof(model.Registrations));

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadRes = await _photoService.AddPhotoAsync(imageFile, "ad-diin/activities");
                        if (uploadRes != null && uploadRes.SecureUrl != null)
                        {
                            model.ImageUrl = uploadRes.SecureUrl.ToString();
                            model.ImagePublicId = uploadRes.PublicId;
                        }
                        else if (uploadRes?.Error != null)
                        {
                            TempData["ErrorMessage"] = $"Cloudinary upload warning: {uploadRes.Error.Message}";
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Cloudinary upload error: {ex.Message}";
                    }
                }

                await _activityService.UpdateAsync(id, model);
                TempData["SuccessMessage"] = "Activity updated successfully!";
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation error: {errors}";
            }
            return RedirectToAction(nameof(Activities));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivityToggle(int id)
        {
            await _activityService.ToggleActiveAsync(id);
            TempData["SuccessMessage"] = "Activity visibility status toggled.";
            return RedirectToAction(nameof(Activities));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivityDelete(int id)
        {
            await _activityService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Activity deleted successfully.";
            return RedirectToAction(nameof(Activities));
        }

        // ================= PROGRAM REGISTRATIONS =================
        public async Task<IActionResult> Registrations(string? status, int? activityId, string? search)
        {
            var list = await _activityService.GetAllRegistrationsAsync(status, activityId, search);
            var activities = await _activityService.GetAllActivitiesAsync();

            var vm = new AdminRegistrationsViewModel
            {
                Registrations = list,
                StatusFilter = status,
                ActivityFilter = activityId,
                SearchQuery = search,
                AvailableActivities = activities
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrationReview(int id, string status, string? adminRemarks)
        {
            var success = await _activityService.ReviewRegistrationAsync(id, status, adminRemarks);
            if (success)
            {
                TempData["SuccessMessage"] = $"Registration status updated to '{status}'.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update registration status.";
            }

            return RedirectToAction(nameof(Registrations));
        }

        // ================= CONTACT MESSAGES =================
        public async Task<IActionResult> Contact()
        {
            var messages = await _contactService.GetAllMessagesAsync();
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactMarkRead(int id)
        {
            await _contactService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Contact));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactReply(int id, string replyText)
        {
            if (!string.IsNullOrWhiteSpace(replyText))
            {
                await _contactService.ReplyMessageAsync(id, replyText.Trim());
                TempData["SuccessMessage"] = "Reply recorded and confirmation email dispatched.";
            }
            return RedirectToAction(nameof(Contact));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactDelete(int id)
        {
            await _contactService.DeleteMessageAsync(id);
            TempData["SuccessMessage"] = "Contact message deleted.";
            return RedirectToAction(nameof(Contact));
        }

        // ================= MESSAGING =================
        public async Task<IActionResult> Messages(int? id)
        {
            var adminUser = await _userManager.GetUserAsync(User);
            if (adminUser == null) return RedirectToAction("Login", "Account");

            var conversations = await _messagingService.GetUserConversationsAsync(adminUser.Id, isAdmin: true);

            Conversation? active = null;
            if (id.HasValue)
            {
                active = await _messagingService.GetConversationWithMessagesAsync(id.Value, adminUser.Id, isAdmin: true);
            }
            else if (conversations.Any())
            {
                active = await _messagingService.GetConversationWithMessagesAsync(conversations.First().Id, adminUser.Id, isAdmin: true);
            }

            ViewBag.ActiveConversation = active;
            return View(conversations);
        }

        // ================= ABOUT CONTENT =================
        public async Task<IActionResult> About()
        {
            var content = await _aboutService.GetContentAsync();
            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> About(AboutContentModel model)
        {
            await _aboutService.UpdateContentAsync(model);
            TempData["SuccessMessage"] = "About Page content updated successfully.";
            return RedirectToAction(nameof(About));
        }
    }
}
