# EShop - Full-Stack .NET E-Commerce Demo

EShop is a clean, modern, full-stack e-commerce sample that highlights backend engineering, frontend development,
cloud-native patterns, and developer experience.

## Overview

This solution uses:

- .NET backend services fronted by a YARP Gateway
- Blazor Server UI for the catalog experience
- SQL Server + EF Core for catalog data
- Azure Blob Storage for product images
- Azure Service Bus for image processing workflows
- .NET Aspire for local orchestration and observability defaults

## Features

- Product catalog with categories and images
- Gateway-mediated API access via YARP
- Image storage in Azure Blob Storage with background processing
- SQL Server + EF Core with migrations
- Blazor Server UI for the catalog experience
- Orchestrated services via .NET Aspire with observability defaults

## Project Structure

- `docs/` - Developer guides and API references (see `docs/quickstart.md` for local setup).
- `src/EShop.AppHost/` - Aspire orchestration for API, frontend, database, and supporting services.
- `src/EShop.API/` - ASP.NET Core catalog API with controllers, services, and mapping profiles.
- `src/EShop.API.Tests/` - API unit tests.
- `src/EShop.BlobStorageInitializer/` - Initializes blob storage containers for local development.
- `src/EShop.Web/` - Blazor Server frontend components, assets, and HTTP clients.
- `src/EShop.Data/` - EF Core DbContext, entity configurations, and migrations.
- `src/EShop.Domain/` - Domain entities and business logic abstractions.
- `src/EShop.Gateway/` - YARP reverse proxy that fronts API calls.
- `src/EShop.ImagesQueueWorker/` - Background worker for image processing queue.
- `src/EShop.Shared/` - Shared representations and helpers consumed across projects.
- `src/EShop.Shared.Tests/` - Shared library unit tests.
- `src/EShop.ServiceDefaults/` - Cross-cutting host defaults (observability, health checks, etc.).
- `src/EShop.MigrationService/` - Service to apply database migrations during orchestration.
- `src/EShop.sln` - .NET solution tying together all projects.

## Quick Start

EShop uses .NET Aspire to orchestrate backend, frontend, and database with a single command.

See the full guide here: [Quick Start](docs/quickstart.md)

## EF Core migrations

Migrations live in the `EShop.Data` class library, while the API project provides the runtime services
and configuration needed to build the model. Use the API project as the startup assembly when
creating or applying migrations:

```bash
# from the repo root
dotnet ef migrations add <MigrationName> -p src/EShop.Data -s src/EShop.API -o Migrations

# apply the migrations to your configured database
dotnet ef database update -p src/EShop.Data -s src/EShop.API
```

This ensures EF Core can locate the `CatalogDbContext` and any related configuration while keeping
the migrations alongside the data layer.

## API

- [Error Handling](docs/api/errorhandling.md)
- [Azure Blob Storage](docs/azure-blob-storage.md)
- [Observability](docs/observability.md)
