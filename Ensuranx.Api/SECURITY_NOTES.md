# Security notes — read before deploying

## Rotate these immediately

The values below were hardcoded in source and committed to git history (`appsettings.json`,
`Controllers/CMSGOVController.cs`). Removing them from the working copy does not remove them
from history — treat all four as compromised regardless of whether the GitHub remote is
currently private, and rotate them at the source:

1. **Gmail app password** for `joinmeshabbi03@gmail.com` — revoke it in the Google Account's
   App Passwords settings and generate a new one.
2. **JWT signing key** (`JWT:Key`) — generate a new random secret. Anyone with the old key could
   forge valid auth tokens for this API.
3. **Two HealthCare.gov Marketplace API keys** — request replacements from the CMS Marketplace
   API developer portal; the old ones should be considered leaked.

## Where the values go now

`appsettings.json` ships with empty placeholders for:
- `CMSGOV:CountyApiKey`
- `CMSGOV:MarketplaceApiKey`
- `JWT:Key`
- `MailSettings:UserName`, `MailSettings:From`, `MailSettings:Password`

Set the real values with one of these, never by editing `appsettings.json` again:

**Local development** — .NET user-secrets (keeps them out of any file that could be committed):
```bash
cd Ensuranx.Api
dotnet user-secrets init
dotnet user-secrets set "CMSGOV:CountyApiKey" "<new-key>"
dotnet user-secrets set "CMSGOV:MarketplaceApiKey" "<new-key>"
dotnet user-secrets set "JWT:Key" "<new-random-secret>"
dotnet user-secrets set "MailSettings:UserName" "<gmail-address>"
dotnet user-secrets set "MailSettings:From" "<gmail-address>"
dotnet user-secrets set "MailSettings:Password" "<new-app-password>"
```

**Production (Azure App Service, or whichever host)** — set these as Application Settings /
environment variables using `__` as the section separator, e.g. `CMSGOV__CountyApiKey`,
`JWT__Key`, `MailSettings__Password`. ASP.NET Core's configuration system reads environment
variables in this format automatically; no code change needed.

## Also true right now

- `ConnectionStrings:DefaultConnection` still points at `Data Source=localhost;...` with
  Windows Integrated Security. That's fine for local dev, but this API has no deployment target
  yet — once one is chosen (Azure SQL, etc.), the connection string needs to move to config the
  same way as the secrets above, not get hardcoded for the new environment either.
- `CMSGOVController.PostHealthInsuranceInfo` previously ignored its `zipCode` parameter and
  always queried a hardcoded ZIP (`27360`); every visitor got North Carolina county results
  regardless of what they typed. Fixed as part of this pass — it now uses the parameter.
