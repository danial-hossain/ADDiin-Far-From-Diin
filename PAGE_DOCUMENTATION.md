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

