# AdDiin Complete Page Documentation

## Document Purpose

This document describes the user-facing pages and supporting Razor views in the AdDiin platform. It is intended for code review, Git commit planning, onboarding, QA, route verification, and feature handover.

The document follows the real MVC structure in `AdDiin/Controllers`, `AdDiin/Views`, `AdDiin/Models`, and `AdDiin/Services`.

## How To Read This Document

Each page description uses the same structure:

- **Route**: The URL a user opens.
- **Controller**: The MVC controller and action that prepare the page.
- **View**: The Razor file that renders the HTML.
- **Model**: The data contract supplied to the view.
- **Access**: Whether the page is public, authenticated, or administrator-only.
- **Inputs**: Forms, query parameters, uploads, or browser data.
- **Actions**: Requests sent after the page is displayed.
- **User flow**: What a normal visitor can do.
- **Commit boundary**: A logical Git change that can be reviewed independently.
- **QA checklist**: Practical checks for this page.

## Global Page Architecture

### MVC request flow

1. A browser requests a public alias or conventional MVC URL.
2. ASP.NET Core routing selects a controller action.
3. The controller loads entities through the application database context or a service.
4. The controller builds a view model where required.
5. Razor renders the matching `.cshtml` view.
6. The shared layout adds navigation, notifications, scripts, and footer content.
7. Browser JavaScript may call an API, controller action, or SignalR hub.
8. The server validates authentication, authorization, antiforgery tokens, and model state.
9. The server stores changes and returns a redirect, partial result, JSON response, or error.

### Shared layouts

All ordinary views start with `_ViewStart.cshtml`, which selects the main `_Layout.cshtml` unless a view overrides it.

Admin views use `_AdminLayout.cshtml` so that administrator navigation and admin-specific support notifications are visible.

`_ViewImports.cshtml` supplies shared namespaces and MVC tag helpers. This keeps individual views shorter and makes strongly typed models available throughout the view tree.

### Authentication states

The application has three important page states:

1. **Anonymous**: The visitor can view public information and begin selected workflows.
2. **Authenticated user**: The visitor can use personal dashboards, reminders, messaging, donations history, and user-owned records.
3. **Administrator**: The visitor can manage users, events, activities, prayer schedules, donations, registrations, Milad requests, and support conversations.

### Shared feedback

Controllers use `TempData` values for success, error, and information messages. `_NotificationToast.cshtml` displays these messages after redirects.

Client-side validation is supplied by `_ValidationScriptsPartial.cshtml` and works with validation attributes on view models.

### Security expectations

State-changing requests should use POST rather than GET. Forms should include antiforgery protection. User-owned records must be checked against the current user identity. Administrator mutations must be protected by the Admin role. File uploads must be validated for size, type, and storage errors.

---

# Part One: Public Platform Pages

## Page 01: Home Dashboard

### Identity

- **Route**: `/`
- **Alternative route**: `/Home/Index`
- **Controller**: `HomeController.Index`
- **View**: `AdDiin/Views/Home/Index.cshtml`
- **Model**: `HomeViewModel`
- **Access**: Public

### Purpose

The home page is the public entry point for AdDiin. It introduces the platform while also showing live or recently loaded religious and community information. It links visitors to prayer times, events, activities, Zakat, donations, My Deen, Diin AI, product analysis, and account features.

### Data displayed

The page combines several data groups:

1. Current or upcoming prayer information.
2. The next prayer and countdown information.
3. Jamaat or prayer schedule data.
4. Upcoming Islamic events.
5. Active community programs.
6. Navigation links to platform modules.
7. Authentication controls supplied by the shared layout.

### Page lifecycle

1. The browser requests `/`.
2. `HomeController.Index` loads the data needed for the landing page.
3. Prayer, event, and activity data are combined into `HomeViewModel`.
4. Razor renders the page sections.
5. The shared layout renders navigation and user-state controls.
6. Browser scripts activate visual interactions and responsive behavior.

### User actions

- Open the prayer-time page.
- Open the Islamic calendar.
- Browse activities and programs.
- Start a donation or Zakat calculation.
- Open the Diin AI assistant.
- Open the Halal product analyzer.
- Register or sign in.
- Navigate to informational pages.

### Failure behavior

If a secondary data source is unavailable, the page should remain renderable with an empty collection or safe fallback. A failure in one optional section should not hide the core navigation.

### QA checklist

- Anonymous visitors can open `/`.
- The page renders when there are no upcoming events.
- The page renders when there are no active programs.
- Next-prayer data is not displayed as a misleading stale value.
- Links point to the named public aliases.
- Authenticated navigation changes correctly after login.
- Mobile navigation does not cover page content.
- Error handling sends unexpected failures to the shared error page.

### Commit boundary

A home-page commit should contain only home composition, home styles, home data loading, or home-specific client behavior. Shared layout changes should be committed separately because they affect every page.

---

## Page 02: About AdDiin

### Identity

- **Route**: `/about`
- **Controller**: `HomeController.About`
- **View**: `AdDiin/Views/Home/About.cshtml`
- **Data source**: `AboutService` and `AdDiin/App_Data/about-content.json`
- **Access**: Public

### Purpose

The About page explains the platform mission, vision, services, and the reason AdDiin exists. It is informational rather than transactional.

### Data flow

1. `HomeController` requests the about content from `IAboutService`.
2. `AboutService` reads the file-backed content.
3. The controller supplies that content to the Razor view.
4. The view presents the content using the public layout.

### User actions

- Read platform information.
- Navigate to active services.
- Return to the home page.
- Open privacy or contact pages.

### QA checklist

- `/about` loads without authentication.
- Missing content does not cause an unhandled null-reference failure.
- Text from the JSON content is encoded safely.
- The page works on mobile.
- Navigation links remain valid after route changes.

### Commit boundary

About content changes and About page layout changes should be separate commits when possible. JSON content edits should not be mixed with unrelated controller changes.

---

## Page 03: SDG 9 Information

### Identity

- **Route**: `/sdg9`
- **Controller**: `HomeController.SDG9`
- **View**: `AdDiin/Views/Home/SDG9.cshtml`
- **Model**: None or view-local content
- **Access**: Public

### Purpose

This page documents how the platform relates to Sustainable Development Goal 9, including innovation, infrastructure, and technology-enabled community services.

### Expected behavior

The page is a read-only informational page. It should not require a user account or database write. It should remain available even if user-specific services are unavailable.

### QA checklist

- `/sdg9` resolves through the named route.
- No login is required.
- Informational links use safe and valid URLs.
- The page remains readable on narrow screens.

### Commit boundary

SDG9 content and design can be reviewed as a documentation or informational-page commit.

---

## Page 04: Privacy Policy

### Identity

- **Route**: `/privacy`
- **Controller**: `HomeController.Privacy`
- **View**: `AdDiin/Views/Home/Privacy.cshtml`
- **Access**: Public

### Purpose

The Privacy page explains data handling, account information, notifications, payments, communication, and external service usage.

### QA checklist

- The page is reachable before account creation.
- Privacy language matches actual data flows.
- Payment and third-party service wording is not contradictory.
- The logout and login links work from this page.

### Commit boundary

Policy text should be committed separately from controller or database implementation work.

---

## Page 05: Public Event List

### Identity

- **Route**: Conventional `/Events/Index`
- **Controller**: `EventsController.Index`
- **View**: `AdDiin/Views/Events/Index.cshtml`
- **Model**: `List<IslamicEvent>`
- **Access**: Public

### Purpose

The event list presents Islamic occasions and community events stored by the application.

### Data shown

Each event may expose a title, event type, Hijri date, Gregorian date, description, visibility state, and a link to details.

### User flow

1. Visitor opens the event list.
2. Controller loads active events.
3. View renders events in a scan-friendly list or card layout.
4. Visitor selects an event.
5. Browser opens the event details route.

### Important route note

The named `/events` route in `Program.cs` currently points to `IslamicCalendarController.Index`, while the conventional `EventsController.Index` route remains available as `/Events/Index`. These two pages should not be treated as identical until routing is intentionally consolidated.

### QA checklist

- Hidden events do not appear publicly.
- Empty event results show a useful empty state.
- Event links carry the correct identifier.
- Dates are formatted consistently.
- The list is usable with keyboard navigation.

---

## Page 06: Event Details

### Identity

- **Route**: `/Events/Details/{id}`
- **Controller**: `EventsController.Details`
- **View**: `AdDiin/Views/Events/Details.cshtml`
- **Model**: `IslamicEvent`
- **Access**: Public

### Purpose

The details page gives a visitor complete information about one Islamic event and may direct the visitor to a Milad or Dua booking flow.

### Data shown

- Event name.
- Event type.
- Gregorian date.
- Hijri date.
- Description.
- Related action links.

### Failure behavior

An unknown identifier should result in a not-found response or a controlled error page, not a blank page or an exception shown to the visitor.

### QA checklist

- Valid event identifiers render correctly.
- Invalid identifiers return a safe not-found result.
- Event details are escaped and safe.
- Booking links preserve relevant context where needed.

---

## Page 07: Prayer Times

### Identity

- **Route**: `/prayer-times`
- **Controller**: `PrayerTimesController.Index`
- **View**: `AdDiin/Views/PrayerTimes/Index.cshtml`
- **Model**: `IEnumerable<PrayerTime>` or page-specific data
- **Access**: Public

### Purpose

The Prayer Times page displays daily prayer schedules and helps visitors identify the next prayer.

### Main features

1. Fard prayer group.
2. Nafl prayer group.
3. Bengali and English labels where provided.
4. Division or location selector.
5. Next-prayer countdown.
6. Live time refresh.
7. Location detection controls.
8. Azan sound controls.
9. Jamaat or display ordering information.

### Request flow

1. The initial page loads prayer records.
2. Browser JavaScript calculates or displays countdown state.
3. The page can call `PrayerTimes.LiveTimes` for refreshed timing data.
4. A visitor can select a division or location.
5. The UI updates without requiring a full page reload where supported.

### Data rules

Prayer records are configured in the database context. Prayer name uniqueness and display ordering are indexed by EF Core. The controller and view should treat missing times as unavailable rather than inventing a value.

### QA checklist

- `/prayer-times` is public.
- Current time is calculated in the correct timezone.
- Countdown handles midnight and next-day prayer transitions.
- Hidden prayer records are not displayed.
- Division selection does not break when the API returns no records.
- Azan audio requires an explicit browser interaction where browser policy requires it.
- Live refresh failures leave the last valid display intact.

### Commit boundary

Keep prayer database seed changes, admin prayer CRUD, public prayer display, and live JavaScript refresh in separate commits when possible.

---

## Page 08: Islamic Calendar

### Identity

- **Route**: `/islamic-calendar`
- **Alternative route**: `/events`
- **Controller**: `IslamicCalendarController.Index`
- **View**: `AdDiin/Views/IslamicCalendar/Index.cshtml`
- **Model**: `IslamicCalendarPageViewModel`
- **Access**: Public page; reminders require authentication

### Purpose

The Islamic Calendar page provides Hijri date context, upcoming occasions, Sunnah fasting guidance, White Days information, countdowns, and reminder controls.

### Main features

- Current Hijri date.
- Gregorian date relationship.
- Upcoming Islamic occasions.
- Sunnah fasting guidance.
- White Days information.
- Countdown to a selected occasion.
- Reminder buttons.

### Reminder flow

1. A visitor selects a reminder button.
2. Browser JavaScript sends a POST request to `SetEventReminder`.
3. The server checks whether the visitor is authenticated.
4. An authenticated user receives or updates a reminder.
5. An anonymous visitor receives a login prompt or redirect.
6. The notification badge may update after a successful request.

### Security rules

Reminder creation is a state-changing operation and must use antiforgery validation or an equivalent secured request strategy. The reminder must be associated with the current user, never with an arbitrary user identifier supplied by the browser.

### QA checklist

- Public calendar content loads for guests.
- Reminder action behaves correctly for guests.
- Duplicate reminders are handled intentionally.
- The page remains usable when there are no upcoming occasions.
- Hijri and Gregorian dates are displayed consistently.
- Notification count updates only after a successful server response.

---

## Page 09: Zakat Calculator

### Identity

- **Route**: `/zakat`
- **Alternative route**: `/zakat-and-donate`
- **Controller**: `ZakatController.Index` and `ZakatController.Calculate`
- **View**: `AdDiin/Views/Zakat/Index.cshtml`
- **Model**: `ZakatCalculatorViewModel`
- **Access**: Public

### Purpose

The Zakat page lets a visitor enter eligible assets and debts, compare the result against a Nisab threshold, and calculate a 2.5 percent payable amount when eligible.

### Input categories

The calculator can include cash, savings, gold, silver, investments, business assets, receivables, debts, and other configured categories. The exact fields are controlled by the view model and view.

### Calculation flow

1. Visitor opens the calculator.
2. Visitor enters numeric asset values.
3. Visitor enters deductible debts where supported.
4. The form posts to `Zakat.Calculate`.
5. The controller validates numeric ranges and model state.
6. The service or controller computes net eligible wealth.
7. The result compares net wealth with Nisab.
8. The page returns eligibility and the payable amount.
9. A donation call-to-action may send the visitor to `/donate`.

### Validation rules

Negative values should be rejected or normalized according to the view model contract. Currency formatting must not be confused with numeric parsing. The page should clearly distinguish total assets, deductions, net wealth, Nisab threshold, eligibility, and payable Zakat.

### QA checklist

- Empty form renders safely.
- Zero values do not cause division or formatting errors.
- Invalid text input produces validation feedback.
- Decimal currency values are handled correctly.
- Calculation result is reproducible for the same input.
- Donation links preserve category context when intended.

---

## Page 10: Focus and Digital Shield

### Identity

- **Routes**: `/focus` and `/focus/shield`
- **Controller**: `FocusController.Index` and `FocusController.Shield`
- **Views**: `AdDiin/Views/Focus/Index.cshtml`, `AdDiin/Views/Focus/Shield.cshtml`
- **Access**: Public

### Purpose

The Focus page provides a digital fasting or concentration timer, blocked-platform controls, extension guidance, and local browser settings. The Shield page displays the state associated with a blocked domain.

### Focus page behavior

- Starts and stops a focus timer.
- Stores selected settings in browser-local state where supported.
- Toggles blocked-platform options.
- Provides extension setup or download information.
- Redirects blocked navigation to the Shield page.

### Shield page behavior

The Shield page reads a `domain` query parameter and shows the visitor which domain was blocked or redirected. The value must be displayed safely and encoded as text.

### QA checklist

- Timer survives normal UI interactions.
- Refresh behavior is intentional.
- Local settings do not expose sensitive data.
- A missing `domain` parameter has a useful fallback.
- Domain text is safely encoded.
- The extension link points to the correct artifact.

---

## Page 11: Halal Product Analyzer

### Identity

- **Route**: `/product-analyzer`
- **Controller**: `ProductAnalyzerController.Index`
- **View**: `AdDiin/Views/ProductAnalyzer/Index.cshtml`
- **Access**: Public
- **API endpoints**: `/api/product-analyzer/analyze`, `/api/product-analyzer/analyze-text`, health endpoint

### Purpose

The Product Analyzer accepts an uploaded product-label image or ingredient text and returns a Halal/Haram analysis with supporting explanations.

### Image flow

1. Visitor selects an image.
2. Browser validates basic file metadata.
3. Browser sends multipart form data to the analysis endpoint.
4. Server validates file type and size.
5. OCR or image analysis extracts product text.
6. The detector service analyzes ingredients.
7. JSON response returns classification, confidence, warnings, and explanation.
8. The browser renders a result panel.

### Text flow

1. Visitor enters ingredient text.
2. Browser sends JSON to `/api/product-analyzer/analyze-text`.
3. Server validates the text length and content.
4. Service returns the classification.
5. Browser displays ingredients and reasoning.

### Security rules

Uploaded files must not be trusted by extension alone. The server should validate content type, length, and any downstream storage behavior. Error responses must not expose provider secrets or raw stack traces.

### QA checklist

- Empty upload is rejected clearly.
- Oversized files are rejected.
- Unsupported file types are rejected.
- Empty ingredient text is rejected.
- API failures produce readable UI feedback.
- Loading state is removed after success or failure.
- Results are escaped before rendering.
- Health status does not expose credentials.

---

# Part Two: Account Pages

## Page 12: Login

### Identity

- **Route**: `/user-login`
- **Alternative route**: `/Account/Login`
- **Controller**: `AccountController.Login`
- **View**: `AdDiin/Views/Account/Login.cshtml`
- **Model**: `LoginViewModel`
- **Access**: Anonymous users; authenticated users may be redirected

### Purpose

The login page authenticates a user with email and password and optionally preserves the session with Remember Me.

### Input fields

- Email address.
- Password.
- Remember Me checkbox.
- Optional local return URL.

### Login flow

1. Visitor opens `/user-login`.
2. The GET action renders the form.
3. The visitor submits credentials.
4. Model validation runs.
5. Identity checks the credentials.
6. A successful user is redirected to the safe return location or home.
7. An administrator may be redirected to the admin dashboard.
8. A failed attempt returns validation or authentication feedback.

### Security rules

Return URLs must be validated as local URLs before redirecting. Passwords must never be logged. Authentication cookies must remain HttpOnly and use the configured expiration policy. Login forms require antiforgery protection on POST.

### QA checklist

- Invalid credentials do not reveal whether an email exists.
- Empty fields show validation messages.
- Remember Me changes cookie persistence as intended.
- Return URL cannot redirect to an external domain.
- Admin and user destinations are correct.
- Already authenticated users are handled consistently.

---

## Page 13: Registration

### Identity

- **Route**: `/user-registration`
- **Controller**: `AccountController.Register`
- **View**: `AdDiin/Views/Account/Register.cshtml`
- **Model**: `RegisterViewModel`
- **Access**: Anonymous users

### Purpose

The registration page creates a new user account, assigns the default User role, and begins email verification.

### Input fields

The form can include name, email, phone, password, password confirmation, and profile-related fields defined by `RegisterViewModel`.

### Registration flow

1. Visitor enters registration information.
2. Server validates model state.
3. Identity checks email uniqueness and password policy.
4. User record is created.
5. Default User role is assigned.
6. Verification code is generated and sent or recorded.
7. Visitor is directed to email verification.

### Failure behavior

Duplicate email, weak password, invalid confirmation, invalid email, and service errors should return the form with useful messages. A partially created account must not leave an inconsistent verification state.

### QA checklist

- Duplicate emails are handled.
- Password confirmation is enforced.
- Password values are never returned to the view after a failed post.
- Role assignment is correct.
- Verification begins only after successful account creation.
- Anti-forgery protection is active.

---

## Page 14: Email Verification

### Identity

- **Route**: `/verify-email`
- **Controller**: `AccountController.VerifyEmail`
- **View**: `AdDiin/Views/Account/VerifyEmail.cshtml`
- **Model**: `VerifyEmailViewModel`
- **Access**: Public workflow page

### Purpose

The page accepts a six-digit verification code and can resend a code when the original expires or is not received.

### Flow

1. Visitor arrives after registration.
2. Email and verification context are retained safely.
3. Visitor enters the code.
4. Server validates ownership, expiry, and code value.
5. Account email-confirmed state is updated.
6. Successful verification signs in the user or redirects to the intended destination.
7. `ResendCode` creates a new code under rate and expiry rules.

### Security rules

Verification codes must be short-lived, single-use where possible, and rate-limited. Error messages should not expose unnecessary account information.

### QA checklist

- Correct code succeeds.
- Wrong code fails safely.
- Expired code fails clearly.
- Resend does not create unlimited valid codes.
- Verified account cannot be verified repeatedly as a new flow.
- Successful verification sends the user to the expected page.

---

## Page 15: User Profile

### Identity

- **Route**: `/user-profile`
- **Controller**: `AccountController.Profile` and `UpdateInfo`
- **View**: `AdDiin/Views/Account/Profile.cshtml`
- **Model**: `UserProfileDashboardViewModel`
- **Access**: Authenticated users

### Purpose

The profile dashboard combines personal information, progress statistics, streaks, badges, and links to personal features.

### Data shown

- Display name and account email.
- Prayer progress.
- Quran progress.
- Dhikr progress.
- Donation totals.
- Streak information.
- Achievement badges.
- Links to password change and My Deen.

### Update flow

1. Authenticated user opens profile.
2. Controller loads only the current user’s data.
3. User edits allowed profile fields.
4. Form posts to `UpdateInfo`.
5. Server validates ownership and model state.
6. User record is updated.
7. Redirect returns to profile with a feedback message.

### Security rules

The user identifier should come from the authenticated principal, not from an editable hidden field. Sensitive fields should not be mass-assigned. Email changes may require re-verification.

### QA checklist

- Anonymous visitor is redirected to login.
- User A cannot see User B statistics.
- Valid profile update persists.
- Invalid profile update preserves useful form values.
- Progress statistics handle zero activity.
- Badge display is stable when there are no badges.

---

## Page 16: Change Password

### Identity

- **Route**: `/Account/ChangePassword`
- **Controller**: `AccountController.ChangePassword`
- **View**: `AdDiin/Views/Account/ChangePassword.cshtml`
- **Model**: `ChangePasswordViewModel`
- **Access**: Authenticated users

### Purpose

Allows the current user to change the account password by submitting the current password and a new password confirmation.

### Flow

1. User opens the page.
2. User enters current password.
3. User enters new password and confirmation.
4. Server checks identity and password policy.
5. Password hash is replaced through Identity.
6. The page redirects to profile or returns validation feedback.

### QA checklist

- Incorrect current password fails.
- Weak new password fails according to policy.
- Confirmation mismatch fails.
- Password values are never displayed.
- Successful change invalidates or refreshes sessions according to Identity policy.

---

## Page 17: Access Denied

### Identity

- **Route**: `/Account/AccessDenied`
- **Controller**: Identity cookie configuration destination
- **View**: `AdDiin/Views/Account/AccessDenied.cshtml`
- **Access**: Public error destination

### Purpose

Explains that the current user does not have permission for the requested page and provides a safe navigation path.

### QA checklist

- Unauthorized users reach this page instead of seeing a server exception.
- The page does not reveal protected resource details.
- Home and login links are valid.

---

# Part Three: Activities and Community Programs

## Page 18: Activity List

### Identity

- **Routes**: `/activities`, `/activities-and-programs`
- **Controller**: `ActivitiesController.Index`
- **View**: `AdDiin/Views/Activities/Index.cshtml`
- **Model**: `IEnumerable<Activity>`
- **Access**: Public

### Purpose

Displays active community programs and supports category and search filtering.

### Query flow

1. Visitor opens the activities route.
2. Optional search and category values arrive as query parameters.
3. Controller filters active records.
4. View renders matching activities.
5. Visitor opens details or the registered-activities page.

### QA checklist

- Search values are safely handled.
- Category filters do not expose inactive records.
- Empty results show an informative state.
- Pagination or large result behavior remains usable where applicable.
- Detail links contain the correct integer identifier.

---

## Page 19: Activity Details

### Identity

- **Route**: `/activities/{id}`
- **Controller**: `ActivitiesController.Details` and `Register`
- **View**: `AdDiin/Views/Activities/Details.cshtml`
- **Model**: `Activity`
- **Access**: Public details; registration requires the intended account state

### Purpose

Shows the full information for one program and lets a user register.

### Data shown

- Program title.
- Image.
- Description.
- Date and time.
- Location.
- Organizer.
- Capacity.
- Registration status.

### Registration flow

1. Visitor opens an activity.
2. Server loads the activity by integer ID.
3. View displays the registration form.
4. Authenticated users may have details prefilled.
5. Visitor submits registration.
6. Server checks activity availability and duplicate registration.
7. Registration is created or a validation message is returned.

### QA checklist

- Nonexistent activity IDs return not found.
- Inactive activities cannot receive new registrations.
- Capacity limits are enforced server-side.
- Duplicate registrations are blocked.
- Guest behavior is consistent with the controller.
- Registration POST has antiforgery protection.

---

## Page 20: My Activities

### Identity

- **Route**: `/my-activities`
- **Controller**: `ActivitiesController.MyActivities`
- **View**: `AdDiin/Views/Activities/MyActivities.cshtml`
- **Model**: `IEnumerable<ProgramRegistration>`
- **Access**: Authenticated users

### Purpose

Lists the signed-in user’s registrations and their current statuses.

### Data isolation

The action must obtain the current user ID from the authenticated identity and filter registrations by that value. A query parameter must not be trusted as the ownership boundary.

### QA checklist

- Anonymous visitors are redirected to login.
- User A cannot see User B registrations.
- Status labels are clear.
- Empty registrations show a useful link to activities.
- Registration dates are formatted consistently.

---

# Part Four: Milad and Dua Booking

## Page 21: Milad Create

### Identity

- **Route**: `/Milad/Create`
- **Controller**: `MiladController.Create`
- **View**: `AdDiin/Views/Milad/Create.cshtml`
- **Model**: `MiladCreateViewModel`
- **Access**: GET may be anonymous; POST requires the intended authenticated state

### Purpose

Allows a user to request a Milad, Mahfil, or Dua service by submitting date, contact, location, and description details.

### Flow

1. Visitor opens the create form.
2. Signed-in information may be prefilled.
3. Visitor enters request details.
4. POST validates the current user and model.
5. Request is stored with a pending status.
6. User is redirected to request history or details.

### QA checklist

- Anonymous GET behavior matches the product expectation.
- Anonymous POST cannot create an ownerless request when ownership is required.
- Date validation prevents invalid or past dates when appropriate.
- Request status begins as pending.
- Antiforgery validation is present.

---

## Page 22: My Milad Requests

### Identity

- **Route**: `/Milad/MyRequests`
- **Controller**: `MiladController.MyRequests`
- **View**: `AdDiin/Views/Milad/MyRequests.cshtml`
- **Model**: `List<MiladRequest>`
- **Access**: Authenticated users

### Purpose

Lists the current user’s requests and exposes edit, details, and pending-only cancellation actions.

### Status behavior

- Pending requests may be edited or cancelled.
- Processed requests should be read-only unless the controller explicitly allows another state.
- Administrator remarks should be visible where appropriate.

### QA checklist

- Owner filtering is server-side.
- Cancel action is POST and antiforgery-protected.
- Completed requests cannot be cancelled accidentally.
- Empty state links to request creation.

---

## Page 23: Milad Details

### Identity

- **Route**: `/Milad/Details/{id}`
- **Controller**: `MiladController.Details`
- **View**: `AdDiin/Views/Milad/Details.cshtml`
- **Model**: `MiladRequest`
- **Access**: Authenticated owner or administrator

### Purpose

Displays one booking request, its status, requester information, date, description, and administrator remarks.

### Authorization

The controller must allow the request owner and authorized administrators only. A valid ID alone must never reveal another user’s request.

### QA checklist

- Non-owner receives not found or forbidden behavior according to policy.
- Administrator can review requests.
- Status and remarks are escaped.
- Unknown IDs are handled safely.

---

## Page 24: Milad Edit

### Identity

- **Route**: `/Milad/Edit/{id}`
- **Controller**: `MiladController.Edit`
- **View**: `AdDiin/Views/Milad/Edit.cshtml`
- **Model**: `MiladCreateViewModel`
- **Access**: Authenticated owner with a pending request

### Purpose

Allows an owner to correct or update a pending request before administrator processing.

### Rules

1. The request must exist.
2. The current user must own it unless an explicit admin rule applies.
3. The request must still be pending.
4. The posted values must pass validation.
5. The owner identity and workflow status must not be overwritten by form values.

### QA checklist

- Processed requests cannot be edited.
- Owner checks happen on both GET and POST.
- Hidden IDs are validated against route and database values.
- Successful edits return to a meaningful page.

### Routing note

The named `/milad` route currently points to `ActivitiesController.Index`, not `MiladController.Index`. The named `/my-milad-requests` route currently points to `ActivitiesController.MyActivities`. These aliases should be reviewed before presenting them as Milad shortcuts.

---

# Part Five: Donation and Payment Pages

## Page 25: Donation Start

### Identity

- **Route**: `/donate`
- **Controller**: `DonateController.Index` and `Initiate`
- **View**: `AdDiin/Views/Donate/Index.cshtml`
- **Model**: `DonationInitiateViewModel`
- **Access**: Public

### Purpose

Begins a donation workflow by selecting a category, entering an amount, and supplying donor information.

### Main fields

- Donation category.
- Amount.
- Donor name.
- Email or contact details.
- Anonymous donation option.
- Optional message or context.

### Payment flow

1. Visitor selects a donation category.
