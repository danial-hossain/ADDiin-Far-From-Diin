# Architecture notes

Ad-Diin is organized as an ASP.NET Core MVC application with a service layer
between controllers and Entity Framework Core. This keeps request handling,
business operations, and persistence responsibilities separate without
introducing a second application framework.

## Request boundaries

- **Controllers** receive HTTP input, enforce action-level authorization and
  antiforgery rules, and select the response or view.
- **Services** coordinate reusable operations such as payments, messaging,
  notifications, external APIs, and user progress.
- **Entities** represent persisted records and navigation relationships.
- **View models** shape data for a specific page and avoid coupling views to
  every database field.
- **Views and static assets** render the user interface under `Views` and
  `wwwroot`.

## Persistence and integrations

`ApplicationDbContext` owns Identity and application entities. Migrations in
`AdDiin/Migrations` describe schema history and should be reviewed whenever an
entity relationship or field changes.

External providers are accessed through dedicated services. Payment callbacks,
email delivery, AI requests, media uploads, and scheduled hadith generation
should remain behind their existing service boundaries so controllers do not
need provider-specific implementation details.

## Background work

The scheduled hadith component is registered as a hosted service. It creates
its own dependency-injection scope for database work rather than reusing a
request-scoped context. This is important because hosted services run outside
the normal HTTP request lifetime.
