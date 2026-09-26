# Scribe

[![Build](https://github.com/mudabs/Scribe/actions/workflows/dotnet.yml/badge.svg)](https://github.com/mudabs/Scribe/actions/workflows/dotnet.yml)

Scribe is an ASP.NET Core MVC application for managing an organization’s inventory and equipment lifecycle.

## Features

- Inventory catalog for brands, categories, models, serial numbers, conditions, and locations
- Asset allocation, deallocation, assignment history, and user/group ownership
- Maintenance and service-history tracking
- Dashboard and inventory reporting views
- CSV, XLS, and XLSX serial-number imports
- Active Directory integration for Windows deployments
- Local cookie authentication and seeded demonstration data for container/VPS deployments

## Technology

- .NET 9 and ASP.NET Core MVC
- Entity Framework Core 9
- SQL Server
- Razor Views, Bootstrap, and jQuery
- ExcelDataReader for spreadsheet imports

## Run with Docker

Docker and Docker Compose are the simplest way to run the demonstration stack. The stack starts Scribe and a persistent SQL Server container, applies EF Core migrations on startup, and exposes the app at http://localhost:8080.

    docker compose up --build -d

The default demonstration credentials are demo / ChangeMe123!. Set DEMO_USERNAME, DEMO_PASSWORD, and SA_PASSWORD before starting the stack to replace them.

Demo data seeding is enabled by default and adds brands, categories, models, devices, users, groups, warranties, maintenance records, and local image assets. Set SEED_DEMO_DATA=false to disable seeding. The seeder is additive and safe to run against an existing database.

Stop the stack with:

    docker compose down

The named database volume is preserved. To remove it as well, use docker compose down -v.

## Run from source

Prerequisites:

- .NET SDK 9.0
- SQL Server, or Docker for the database container
- EF Core CLI tools if you need to manage migrations

Restore and build:

    dotnet restore
    dotnet build

Configure ConnectionStrings:DefaultConnection for the target SQL Server, then apply migrations and run the app:

    dotnet ef database update
    dotnet run

## Authentication

The Windows deployment path supports Active Directory. The Docker/VPS demonstration path uses local cookie authentication and does not require Windows Authentication or an Active Directory group lookup.

For the deployed VPS instance, the username is demo. Its password is stored in the protected /opt/apps/scribe/.env file and is intentionally not committed to the repository.

## Deployment

The deployed demonstration instance is available at https://scribe.munashemudabura.com. HTTPS terminates at the public IONOS gateway and is forwarded privately over Tailscale to the Scribe container on vps01; SQL Server is not publicly exposed.

The IONOS site configuration used for this deployment is in the [deployment configuration](deploy/ionos/scribe.munashemudabura.com.conf).

## Verification

The repository currently contains the application project but no dedicated automated test project. The CI workflow restores dependencies, builds the solution with .NET 9, and runs dotnet test so tests can be added later without changing the workflow.

    dotnet build
    dotnet test --no-build

## Repository layout

- Controllers/ — MVC controllers
- Data/ — EF Core context and runtime demo seeding
- Migrations/ — EF Core database migrations
- Models/ — domain models
- Services/ and Infrastructure/ — application services and view components
- Views/ — Razor views
- wwwroot/ — static assets and seeded brand/model images
- deploy/ — deployment configuration
