# Database provider switch: SQL Server → PostgreSQL (Supabase)

This backend was originally built against SQL Server. Switching to Supabase
(managed Postgres) required a real code change, made without a .NET SDK
available to compile or test it — read this before deploying.

## What changed

- `Ensuranx.Api.csproj`, `Ensuranx.Application.csproj`,
  `Ensuranx.Infrastructure.csproj`: package reference swapped from
  `Microsoft.EntityFrameworkCore.SqlServer` to
  `Npgsql.EntityFrameworkCore.PostgreSQL` (7.0.18, the latest 7.0.x release
  compatible with this project's EF Core 7.0.9).
- `Program.cs`: `options.UseSqlServer(...)` → `options.UseNpgsql(...)`.
- **The three existing migrations were deleted.** They contained SQL
  Server-specific types and behavior (`nvarchar`, `datetime2`, identity
  columns) that do not translate to Postgres. Hand-editing them to "convert"
  would be guesswork without a way to verify it — safer to regenerate them
  cleanly against the new provider.

## What still needs to happen before first deploy

A fresh initial migration has to be generated against Postgres. This
requires the .NET SDK and EF Core tools, neither of which exist on the
machine this change was made on. Do this from Azure Cloud Shell (it has
both preinstalled) or any machine with the .NET 7 SDK:

```bash
cd Ensuranx.Api
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef migrations add InitialCreate --project ../Ensuranx.Infrastructure --startup-project .
dotnet ef database update --project ../Ensuranx.Infrastructure --startup-project .
```

The `ConnectionStrings:DefaultConnection` value (set via user-secrets or
environment variables per `SECURITY_NOTES.md`) needs to be a Postgres
connection string from Supabase's dashboard (Project Settings → Database →
Connection string) before running the commands above, e.g.:

```
Host=<project>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<your-db-password>;SSL Mode=Require;Trust Server Certificate=true
```

## Also worth knowing

`CMSGOV:apikey` and the CMSGOV controller's HealthCare.gov calls are
unaffected by this change — they don't touch the database at all.
