# Local Calling Guide

An ASP.NET Core MVC app that looks up the rate center for a 6-digit NPA-NXX or a North American phone number using the Local Calling Guide XML endpoint.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Network access to `https://localcallingguide.com/`

## Local run

```bash
dotnet restore WebApplication3.sln
dotnet run --project WebApplication3/WebApplication3.csproj
```

Open the URL printed by `dotnet run` (usually `https://localhost:7xxx`).

The app accepts:
- 6-digit NPA-NXX (e.g. `212555`)
- 10-digit North American numbers
- 11-digit numbers starting with `1`
- Separators: new line, comma, semicolon, or `|`
- Up to 25 entries at a time

## Tests

```bash
dotnet test WebApplication3.Tests/WebApplication3.Tests.csproj
```

## Deploy to Azure App Service (via GitHub)

This is a server-rendered ASP.NET Core app. Host it on **Azure App Service** (Linux recommended), not Static Web Apps.

### 1. Create the App Service

1. Azure Portal → **Create a resource** → **Web App**.
2. Runtime stack: **.NET 10 (LTS)** (or latest available).
3. Operating system: **Linux**.
4. Pricing plan: Free (F1) or Basic is fine for testing.
5. Note the app name (example used below: `lcg-rate-center`).

### 2. Configure Application Settings

In the Web App → **Configuration** → **Application settings**, add:

| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `LocalCallingGuide__BaseUrl` | `https://localcallingguide.com/` |

Save and restart if prompted. Enable **HTTPS Only** under General settings.

### 3. Set up GitHub Actions deployment

The repo includes `.github/workflows/azure-webapp.yml`.

1. In the Azure Portal, open your Web App → **Deployment Center**.
2. Source: **GitHub**.
3. Authorize and select your repository and branch (`main` or `master`).
4. Azure will create a service principal and show you three secrets. Add them to your GitHub repo under **Settings → Secrets and variables → Actions**:

   - `AZURE_CLIENT_ID`
   - `AZURE_TENANT_ID`
   - `AZURE_SUBSCRIPTION_ID`

5. Edit `.github/workflows/azure-webapp.yml` and set `AZURE_WEBAPP_NAME` to your actual App Service name.
6. Commit and push. The workflow will build, test, and deploy on pushes to `main`/`master`.

### 4. Verify

- Visit `https://<your-app-name>.azurewebsites.net`
- Try a prefix such as `212555` or a full number.
- Check **Deployment Center → Logs** and **Log stream** if the site does not update.

## Notes

- The upstream URL can be overridden with the `LocalCallingGuide__BaseUrl` application setting.
- The app makes outbound HTTPS calls to `localcallingguide.com` — ensure the App Service can reach the internet.
- No database or inbound secrets are required.
