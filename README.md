# Employee Management

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC%20%2B%20API-512BD4)
![Database](https://img.shields.io/badge/Database-SQLite-003B57?logo=sqlite&logoColor=white)

A role-based employee and department management system built with ASP.NET Core 10. The solution pairs an MVC web application with a JWT-secured API, layered application services, and EF Core persistence.

## Contents

- [Highlights](#highlights)
- [Architecture](#architecture)
- [Permissions](#permissions)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Build And Test](#build-and-test)
- [API Reference](#api-reference)
- [Production Checklist](#production-checklist)

## Highlights

- Search and filter the employee directory by name, email, department, and status; paginate results and select page size.
- Manage employee records and departments with role-appropriate create, edit, and delete actions.
- Review workforce totals, active/inactive counts, and average salary on the dashboard.
- Open department-specific employee reports.
- Provision HR and Employee accounts according to the role rules below.
- Let signed-in users update their own name and phone number or change their password.
- Review recent successful changes in the Admin-only audit history.
- Get consistent API validation and error responses, plus a database readiness endpoint.

## Architecture

```text
EmployeeManagement.MVC  -->  EmployeeManagement.API  -->  Application services
									  |                          |
									  +------ Infrastructure -----+
												 |
									  EF Core / SQLite
```

| Project | Responsibility |
|---|---|
| `EmployeeManagement.MVC` | Browser UI, cookie authentication, and API client. |
| `EmployeeManagement.API` | JWT authentication, authorization, HTTP endpoints, health checks, and auditing. |
| `EmployeeManagement.Application` | DTOs, service contracts, validation, and business logic. |
| `EmployeeManagement.Domain` | Employee, department, and audit entities. |
| `EmployeeManagement.Infrastructure` | EF Core context, SQLite provider, migrations, and repositories. |
| `EmployeeManagement.Tests` | Service, repository, migration, and API authorization tests. |

## Permissions

| Capability | Admin | HR | Employee |
|---|:---:|:---:|:---:|
| View employee directory and details | Yes | Yes | Yes |
| Create, edit, or delete employee records | Yes | Yes | No |
| View departments and employee reports | Yes | Yes | No |
| Create, edit, or delete departments | Yes | No | No |
| Create an HR account | Yes | No | No |
| Create an Employee account | Yes | Yes | No |
| Edit own profile or password | Yes | Yes | Yes |
| View audit history | Yes | No | No |

Public registration is disabled. Login is the only anonymous authentication route; `/health` is anonymous by design. Employee-role users can view the full employee directory and details, including the fields returned by those endpoints.

## Quick Start

### 1. Configure local secrets

From the repository root, use .NET User Secrets for the API. Generate a unique signing key of at least 32 bytes and choose a strong initial Admin password that meets the Identity password policy.

```powershell
dotnet user-secrets set "Jwt:Key" "<unique signing secret>" --project EmployeeManagement.API
dotnet user-secrets set "Jwt:Issuer" "EmployeeManagementAPI" --project EmployeeManagement.API
dotnet user-secrets set "Jwt:Audience" "EmployeeManagementClient" --project EmployeeManagement.API
dotnet user-secrets set "Jwt:ExpirationMinutes" "60" --project EmployeeManagement.API
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com" --project EmployeeManagement.API
dotnet user-secrets set "SeedAdmin:Password" "<strong initial password>" --project EmployeeManagement.API
```

On first startup, the API creates the configured Admin account if it does not exist. After bootstrap, keep the email configuration but remove the password from the runtime configuration. Existing Admin accounts are promoted without needing their password on each start.

### 2. Start the applications

Run the API and MVC application in separate terminals:

```powershell
dotnet run --project EmployeeManagement.API --launch-profile http
dotnet run --project EmployeeManagement.MVC
```

The API listens on `http://localhost:5235`. The MVC application uses the API URL in `EmployeeManagement.MVC/appsettings.json`; adjust it when using a different local port. Development startup applies pending EF migrations automatically.

### 3. Open the app

Open the MVC URL printed at startup, then sign in using the configured Admin account. Admins can create HR and Employee accounts; HR users can create Employee accounts.

## Configuration

Never commit credentials or production secrets to JSON configuration files.

In production, inject these settings through the hosting platform's secret/configuration service:

| Setting | Purpose |
|---|---|
| `Jwt__Key` | JWT signing key; use a unique secret of at least 32 bytes. |
| `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationMinutes` | JWT validation and lifetime settings. |
| `ConnectionStrings__DefaultConnection` | Persistent SQLite database path. |
| `SeedAdmin__Email`, `SeedAdmin__Password` | Initial Admin bootstrap; password is needed only if the account does not exist yet. |
| `ApiSettings__BaseUrl` | MVC-to-API URL; production requires HTTPS. |

The application currently uses SQLite. Production storage must be persistent, backed up, and protected with appropriate filesystem access controls. EF migrations run automatically only in Development; review and apply migrations as a deployment step in production.

## Build And Test

```powershell
dotnet build EmployeeManagement.sln --configuration Release
dotnet test EmployeeManagement.Tests/EmployeeManagement.Tests.csproj --configuration Release
```

Integration tests use an isolated in-memory SQLite database and do not modify the development database. The solution includes migration-backed API tests and role checks across protected endpoints.

## API Reference

See [API.md](API.md) for routes, authorization requirements, query parameters, and error formats.

In Development, OpenAPI is available at `/openapi/v1.json`. The anonymous readiness check is `/health`; it returns `503` when the database cannot be reached.

## Production Checklist

- Provide persistent database storage, backups, and a tested restore procedure.
- Store JWT and bootstrap credentials in the hosting platform's secret manager.
- Confirm the first Admin account exists, then remove its bootstrap password from runtime settings.
- Review and apply EF migrations before routing traffic to the new release.
- Serve MVC over HTTPS; production authentication cookies require Secure transport.
- Restrict API network exposure to trusted clients where practical.
- Monitor application logs and `/health`, and configure alerting and log retention.
- Define an audit-log retention policy; audit history is limited to Admin users.
