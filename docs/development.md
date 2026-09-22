# Development guide

## Application flow

The application follows the ASP.NET Core MVC pattern:

1. A request enters through a controller action.
2. The controller validates input and coordinates the operation.
3. Services contain reusable business and integration logic.
4. Entity Framework Core persists application data through
   `ApplicationDbContext`.
5. Razor views render the response using view models where a page needs a
   presentation-specific shape.

When adding a feature, keep database entities in `Models/Entities`, page-specific
models in `Models/ViewModels`, and external-provider calls behind a service.

## Database changes

Database schema changes are managed with Entity Framework Core migrations.
Create a migration only after confirming that the model change is required:

```powershell
dotnet ef migrations add DescribeTheChange --project AdDiin/AdDiin.csproj
dotnet ef database update --project AdDiin/AdDiin.csproj
```

Review generated migration files before committing them. Do not edit an
already-applied migration to change production history; create a follow-up
migration instead.

## UI changes

Shared layout and validation behavior live under `Views/Shared`. Feature views
should use the existing layout and validation partials. Static styles and
scripts belong under `AdDiin/wwwroot`, grouped by responsibility rather than
duplicated inside individual views.
