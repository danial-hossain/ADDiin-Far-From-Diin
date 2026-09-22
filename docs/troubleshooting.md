# Troubleshooting

## The application cannot connect to SQL Server

Verify `ConnectionStrings:DefaultConnection` and confirm that the database
server is reachable from the machine running the application. For environment
variables, use `ConnectionStrings__DefaultConnection` with two underscores.
Check that the configured database user has permission to apply migrations and
read or write application data.

## Email or verification messages do not arrive

Confirm the SMTP host, port, username, password, and sender address. Gmail
accounts generally require an app password when two-step verification is
enabled. Review application logs for the provider response rather than
resending repeatedly.

## An integration feature is unavailable

Cloudinary, SSLCommerz, Diin AI, and the halal detector each require their own
configuration. Check the matching section in `appsettings.json` and compare
the environment-variable names with
`render-environment-variables.example.txt`. A placeholder value is not a
usable credential or endpoint.

## The published site uses the wrong port

For container hosting, provide the port expected by the platform through the
`PORT` environment variable. The repository Dockerfile defaults to port `8080`
when that variable is absent.
