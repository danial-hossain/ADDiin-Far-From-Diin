# Ad-Diin

## Islamic Development Platform

[![Build and deploy](https://github.com/danial-hossain/ADDiin-Far-From-Diin/actions/workflows/deploy-somee.yml/badge.svg?branch=developer)](https://github.com/danial-hossain/ADDiin-Far-From-Diin/actions/workflows/deploy-somee.yml)
[![Live site](https://img.shields.io/badge/live%20site-addiin.somee.com-0f766e?logo=googlechrome&logoColor=white)](http://addiin.somee.com/)
[![Render](https://img.shields.io/badge/render-live-46e3b7?logo=render&logoColor=111111)](https://addiin-far-from-diin.onrender.com/)

**Live site:** [addiin.somee.com](http://addiin.somee.com/)

**Render deployment:** [addiin-far-from-diin.onrender.com](https://addiin-far-from-diin.onrender.com/)

Ad-Diin is a community-focused Islamic development platform that brings worship,
learning, community programs, communication, and transparent support services
into one web application. It is designed to help individuals, families,
mosques, and administrators access useful Islamic tools while improving the
management of community activities.

The platform is built as an ASP.NET Core MVC application with a responsive
Razor UI, role-based administration, Entity Framework Core, and SQL Server.

## Deployment

The `developer` branch is configured for deployment to Somee through GitHub
Actions. Each successful workflow run is recorded by GitHub under the
`production` environment and links to the live site.

To enable automatic deployment, add these repository secrets under
**Settings → Secrets and variables → Actions**:

- `SOMEE_FTP_SERVER`
- `SOMEE_FTP_USERNAME`
- `SOMEE_FTP_PASSWORD`
- `SOMEE_FTP_SERVER_DIR`

After adding the secrets, push to `developer` or run **Deploy to Somee** from
the repository's **Actions** tab.

## Platform capabilities

### Worship and Islamic learning

- Prayer-time schedules with countdown and Jamaat information.
- Islamic calendar and community event discovery.
- My Deen user area for personal Islamic resources.
- Diin AI, an Islamic learning assistant for questions and guided exploration.
- Halal and Haram product analysis from product text.
- Zakat calculation support.

### Community and mosque services

- Islamic activities, programs, and event registration.
- Milad, Mahfil, and Dua request workflows.
- User profiles, email verification, notifications, and secure authentication.
- Direct messaging and real-time support chat.
- Contact and community communication flows.

### Donations and transparency

- Online donation workflow with success, failure, cancellation, and receipt
  pages.
- Personal donation history.
- Support for payment processing and transaction tracking.
- Administrative visibility into donations and community activities.

### Administration

- Dashboard for managing users, roles, registrations, activities, events,
  prayer times, Milad requests, donations, and messages.
- Database-backed content and operational management.
- Seeded development data and automatic database initialization.
- Role-protected administrative routes.

## Technology stack

| Area | Technology |
| --- | --- |
| Framework | ASP.NET Core MVC on .NET 9 |
| Language | C# |
| UI | Razor Views, HTML5, CSS3, JavaScript, Bootstrap 5.3 |
| Data | Microsoft SQL Server |
| ORM | Entity Framework Core 9 with Code First migrations |
| Authentication | ASP.NET Core Identity |
| Real-time features | SignalR |
| Media | Cloudinary |
| Payments | SSLCommerz integration |
| Architecture | MVC, dependency injection, service layer |

## Project structure

```text
AdDiin/
├── Controllers/       MVC controllers for public, user, and admin features
├── Data/              DbContext, database initialization, and migrations
├── Hubs/              SignalR hubs for real-time communication
├── Models/
│   ├── Entities/      Database entities and domain models
│   └── ViewModels/    Strongly typed UI models
├── Services/          Business logic and external service integrations
├── Views/             Razor page templates
└── wwwroot/           CSS, JavaScript, images, and static assets
```

## Running locally

### Prerequisites

- .NET 9 SDK
- SQL Server, SQL Server Express, or LocalDB
- Visual Studio 2022 or another .NET-compatible IDE
- Optional: Cloudinary, SSLCommerz, SMTP, and AI service credentials for
  features that depend on external providers

### Configure the application

1. Clone the repository and open `AdDiin.sln`.
2. Review `AdDiin/appsettings.json`.
3. Keep credentials and API keys out of source control. Use user secrets or
   environment variables for local and production secrets.
4. Set `ConnectionStrings:DefaultConnection` to a SQL Server database.

The application includes a LocalDB fallback connection for development. Database
initialization and seed data are handled when the application starts.

### Run with the .NET CLI

```powershell
dotnet restore
dotnet build
dotnet ef database update --project AdDiin/AdDiin.csproj
dotnet run --project AdDiin/AdDiin.csproj
```

Open the HTTPS URL printed by the application, commonly
`https://localhost:7000`.

### Run with Visual Studio

1. Open `AdDiin.sln` in Visual Studio 2022.
2. Restore NuGet packages if prompted.
3. Select the `AdDiin` project and press **F5**.
4. Allow the application to initialize the database on first launch.

## Important routes

| Area | Route |
| --- | --- |
| Home | `/` |
| Prayer times | `/prayer-times` |
| Islamic calendar | `/islamic-calendar` |
| Activities and programs | `/activities-and-programs` |
| Zakat | `/zakat` |
| Donations | `/donate` |
| Diin AI | `/diin-ai` |
| Product analyzer | `/product-analyzer` |
| Messaging | `/messaging` |
| User login | `/user-login` |
| User registration | `/user-registration` |
| Admin dashboard | `/admin-dashboard` |

## Security and responsible use

- ASP.NET Core Identity manages password hashing and authentication.
- Role-based authorization protects administrative operations.
- Anti-forgery protection is used for state-changing forms.
- Entity Framework Core parameterizes database queries.
- Secrets should be supplied through user secrets, environment variables, or
  a managed secret store rather than committed to Git.
- Diin AI is an educational aid, not a substitute for qualified scholars or
  professional advice. Users should verify sensitive religious, legal, health,
  and financial questions with trusted experts.

## Project purpose and SDG alignment

Ad-Diin supports the UN Sustainable Development Goal 9 by applying digital
infrastructure and software innovation to Islamic community services. The
platform helps make prayer information, learning resources, community programs,
communication, and donation workflows more accessible and organized.

## Team

| Name | Responsibility |
| --- | --- |
| Danial Hossain Dani | Team Lead |
| Toufikul Alam Yame | Backend Developer |
| Md Salahuddin Yousuf | Frontend Developer |
