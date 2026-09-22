# Testing guide

## Build and test commands

Run these commands from the repository root:

```powershell
dotnet restore
dotnet build AdDiin.sln --no-restore
dotnet test AdDiin.sln --no-build --verbosity normal
```

The CI workflow performs restore, build, and test on pushes and pull requests
to `main`. A successful build does not replace functional testing of database,
email, payment, and external AI integrations.

## Manual smoke checks

For changes affecting authenticated pages, verify registration, email
verification, login, logout, and access-denied behavior. For donation changes,
check pending, success, failure, cancellation, and receipt paths in the
configured payment mode.

For Diin AI and Product Analyzer changes, verify both successful responses and
provider-unavailable states. Confirm that user-safe fallback messages appear
without exposing credentials or raw provider secrets.

## Database-aware checks

Use a development database when testing migrations, seed data, Identity roles,
or scheduled-hadith persistence. Review generated migrations before applying
them and avoid using production credentials during local tests.
