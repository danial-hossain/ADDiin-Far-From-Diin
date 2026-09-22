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
