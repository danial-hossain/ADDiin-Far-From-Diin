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

<!--
Feature branch Railway deployment — intentionally inactive.

### Railway

The repository also includes a `Dockerfile` and `railway.toml` for deploying
the complete ASP.NET Core application to Railway. In Railway, create a
project from this GitHub repository, select the `developer` branch, and deploy
the service using the detected Dockerfile.

Add the production values as Railway variables using the same names as the
configuration keys, for example
`ConnectionStrings__DefaultConnection` and `EmailSettings__Password`. Do not
commit production credentials to GitHub.
Railway automatically provides the `PORT` variable used by the container.
-->

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