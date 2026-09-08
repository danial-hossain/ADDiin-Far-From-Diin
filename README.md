# 🕌 Ad-Diin: Islamic Development Platform

> **Course:** AUST CSE 3200 — Software Development-V  
> **Target IDE:** Visual Studio 2022  
> **Framework:** ASP.NET Core MVC (.NET 9.0 / .NET 8.0)  
> **Language:** C#  
> **Database:** Microsoft SQL Server with Entity Framework Core (Code First + Migrations)  
> **Authentication:** ASP.NET Core Identity  
> **SDG Alignment:** UN SDG 9 (Industry, Innovation & Infrastructure)

---

## 👥 Team Members

| Roll Number | Name | Email | Role |
|---|---|---|---|
| **20230104058** | Danial Hossain Dani | danialhossaindani@gmail.com | Team Lead |
| **20230104070** | Toufikul Alam Yame | toufikul.alam30@gmail.com | Backend Developer |
| — | Md Salahuddin Yousuf | sagor200301714643817@gmail.com | Frontend Developer |

---

## 📌 Project Overview & Purpose

**Ad-Diin** is a centralized smart mosque management and community engagement platform built to digitize, streamline, and bring transparency to mosque operations. The system eliminates manual registers and opaque cash collections by providing:
- Real-time automated prayer and Jamaat schedules with countdown indicators.
- Interactive Zakat calculation and transparent online donation processing.
- Online booking system for family Milad, Mahfil, and special Dua requests.
- Community announcement boards, welfare programs, and Islamic calendar events.
- **Diin AI**: An intelligent Islamic learning assistant delivering verified guidance backed by Qur'an and Sahih Hadith citations.
- Full-featured administrative dashboard for user roles, prayer times, events, activities, inquiries, and financial audits.

---

## 🎯 UN SDG 9 Alignment (Industry, Innovation & Infrastructure)

Ad-Diin directly addresses **Sustainable Development Goal 9 (SDG 9: Industry, Innovation, and Infrastructure)**:
1. **Target 9.c (Universal Access to Information & ICT):** Digitizing mosque administration and enabling universal access to religious and civic information.
2. **Target 9.1 (Resilient & Transparent Infrastructure):** Transforming traditional community governance with auditable financial transactions, verifiable donation tracking tokens (TranId), and transparent fund allocations.
3. **Smart Educational Innovation:** Democratizing authentic Islamic learning through Diin AI.

---

## 🛠 Technology Stack

- **IDE:** Visual Studio 2022
- **Language:** C#
- **Web Framework:** ASP.NET Core MVC
- **Database Engine:** Microsoft SQL Server (LocalDB / Express / Enterprise)
- **ORM:** Entity Framework Core
- **Database Approach:** Code-First with EF Core Migrations
- **Authentication & Security:** ASP.NET Core Identity (PBKDF2 Password Hashing, Anti-Forgery Tokens, Role-Based Authorization)
- **Frontend Presentation:** Razor Views (.cshtml), HTML5, CSS3, JavaScript, Bootstrap 5.3, Bootstrap Icons
- **Architecture:** Multi-Tier MVC with Dependency Injection & Service Layer

---

## 🧩 Architectural System Design

`
AdDiin/
├── Controllers/              # MVC Action Controllers
│   ├── HomeController.cs     # Public Home, About, Contact, SDG9, Privacy
│   ├── AccountController.cs  # Authentication, Identity, Registration, Profile
│   ├── PrayerTimesController.cs
│   ├── EventsController.cs
│   ├── ActivitiesController.cs
│   ├── ZakatController.cs
│   ├── DonateController.cs
│   ├── MiladController.cs
│   ├── MessagesController.cs
│   ├── DiinAIController.cs
│   └── AdminController.cs    # Executive Management Portal
├── Data/
│   ├── ApplicationDbContext.cs # EF Core DbContext with Fluent API mappings
│   ├── DbInitializer.cs       # Automatic DB migration & Seed data
│   └── Migrations/            # EF Core Code-First Migrations
├── Models/
│   ├── Entities/             # Domain Entities (User, PrayerTime, Event, Milad, Donation, etc.)
│   └── ViewModels/           # Strongly-Typed ViewModels
├── Services/                 # Business Logic & Service Interfaces
│   ├── IPrayerTimeService.cs
│   ├── IDonationService.cs
│   ├── IMiladService.cs
│   ├── IIslamicEventService.cs
│   ├── IActivityService.cs
│   ├── IMessagingService.cs
│   ├── IContactService.cs
│   ├── IDiinAIService.cs     # Islamic Knowledge AI Engine
│   ├── IEmailVerificationService.cs
│   └── IAboutService.cs
├── Views/                    # Razor View Templates
└── wwwroot/                  # Static Assets (CSS, JS, Images)
`

---

## 🔑 Default Credentials (Auto-Seeded)

The system automatically initializes and seeds default roles and accounts on first launch:

| Role | Email | Password | Access Level |
|---|---|---|---|
| **Administrator** | dmin@addiin.com | Admin@123 | Full Admin Management Portal (/Admin/Dashboard) |
| **Demo User** | 	est@test.com | User@123 | Public & User Features (/Account/Profile, /Milad, /Donate) |

---

## 🚀 How to Run in Visual Studio 2022

### Method 1: Visual Studio 2022 GUI
1. Open Visual Studio 2022.
2. Click **Open a project or solution** and select AdDiin.sln.
3. Press F5 (or click the green **Start** button).
4. Visual Studio will restore NuGet packages, start Microsoft SQL Server LocalDB, apply EF Core migrations, seed initial data, and launch the browser.

### Method 2: .NET CLI
`powershell
# Restore & Build
dotnet restore
dotnet build

# Apply Database Migrations (Automatic on startup, or manually):
dotnet ef database update --project AdDiin/AdDiin.csproj

# Run Application
dotnet run --project AdDiin/AdDiin.csproj
`

Open your browser at: https://localhost:7000 (or http://localhost:5000).

---

## 🛡️ Security & Quality Standards
- **Data Protection:** Passwords secured with salted PBKDF2 hashing via ASP.NET Core Identity.
- **CSRF Defense:** [ValidateAntiForgeryToken] applied across all forms and state modifications.
- **Role-Based Authorization:** [Authorize(Roles = "Admin")] protecting all admin capabilities.
- **Database Safety:** Parameterized LINQ queries preventing SQL injection vulnerabilities.
