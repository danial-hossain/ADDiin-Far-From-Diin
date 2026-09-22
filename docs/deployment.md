# Deployment notes

Ad-Diin can be published as a standard ASP.NET Core application or built with
the repository Dockerfile. Deployment requires the same application settings
as local development, plus provider credentials for any enabled integrations.

## Somee

The manual Somee workflow is defined in
`.github/workflows/deploy-somee.yml`. It restores the solution, publishes the
`AdDiin` project in Release mode, and uploads the published output over FTP.

Configure these repository secrets before running the workflow:

- `SOMEE_FTP_SERVER`
- `SOMEE_FTP_USERNAME`
- `SOMEE_FTP_PASSWORD`
- `SOMEE_FTP_SERVER_DIR`

The workflow is manually dispatched from the Actions tab. Keep the production
environment protection rules enabled so deployment remains an explicit action.

## Render or container hosting

The `Dockerfile` publishes the application and starts it on the port supplied
by the `PORT` environment variable, defaulting to `8080`. Container platforms
should provide a SQL Server connection string and any required integration
settings through their secret or environment-variable facilities.

Use `render-environment-variables.example.txt` as a key reference only. It is
safe to copy the names, but never commit real values from that template.
