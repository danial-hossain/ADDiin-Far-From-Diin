# Release checklist

Use this checklist before publishing a deployment:

- Confirm the intended commit is on `main` and the worktree is clean.
- Run `dotnet restore` and `dotnet build AdDiin.sln --no-restore`.
- Review build warnings and confirm no new warning was introduced.
- Verify the production connection string and provider settings are supplied
  through secrets or environment variables.
- Confirm payment mode and callback URLs match the target environment.
- Confirm SMTP, Cloudinary, Diin AI, and halal-detector credentials are present
  when their features are enabled.
- Apply reviewed Entity Framework migrations before exercising new schema paths.
- Smoke-test authentication, the home page, prayer times, donations, messaging,
  Diin AI, and Product Analyzer.
- Review deployment logs for startup, migration, and external-provider errors.
- Confirm the deployed health endpoints return the expected connectivity status.

Never use real production credentials in local configuration files or examples.
