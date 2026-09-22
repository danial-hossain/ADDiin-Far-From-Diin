# Configuration reference

The application reads configuration through ASP.NET Core's normal precedence
rules. Local JSON settings provide defaults, while environment variables or
user secrets should supply deployment-specific values and credentials.

## Core settings

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection used by Entity Framework Core |
| `AllowedHosts` | Host filtering for ASP.NET Core |
| `AISettings:TimeoutSeconds` | HTTP timeout shared by AI-related clients |

## External integrations

| Key | Purpose |
| --- | --- |
| `EmailSettings:*` | SMTP delivery for verification messages |
| `CloudinarySettings:*` | Image upload and media-management credentials |
| `SslCommerzSettings:*` | Donation gateway mode, credentials, and callback URLs |
| `DiinAI:BackendUrl` | Diin AI service endpoint |
| `HalalDetector:BackendUrl` | Product analyzer service endpoint |

Nested keys become environment variables by replacing `:` with `__`, for
example `DiinAI__BackendUrl`. The repository also supports the uppercase
environment-variable aliases used by the external-service classes.

## Secret handling

Never commit real SMTP passwords, Cloudinary secrets, payment passwords, API
keys, or private database credentials. Use user secrets for local development
and repository or hosting-provider secrets for CI and production. The
`render-environment-variables.example.txt` file documents variable names only;
replace its placeholders outside source control.

## Environment differences

Development can use LocalDB and test-mode payment settings. Production should
provide a managed SQL Server connection, production provider endpoints, secure
cookie and HTTPS settings, and credentials through the hosting platform's
secret store.
