# Full-Stack .NET E-Commerce shop

EShop is a clean, modern, full-stack e-commerce sample that highlights backend engineering, 
frontend development, cloud-native patterns, and developer experience.

## Overview

This solution uses:

- .NET backend services fronted by a YARP Gateway
- Blazor Server UI for the catalog experience
- SQL Server + EF Core for catalog data
- Azure Blob Storage for product images
- Azure Service Bus for image processing workflows
- .NET Aspire for local orchestration and observability defaults

## Architecture

[Architecture Overview](docs/architecture.md)

## Features

- Product catalog with categories and images
- Gateway-mediated API access via YARP
- Image storage in Azure Blob Storage with background processing
- SQL Server + EF Core with migrations
- Blazor Server UI for the catalog experience
- Orchestrated services via .NET Aspire with observability defaults

## Project Structure

- `.github/` - CI workflows and repo automation.
- `docs/` - Developer guides and API references (see `docs/quickstart.md` for local setup).
- `src/EShop.API.Tests/` - API unit tests.
- `src/EShop.API/` - ASP.NET Core catalog API with controllers, services, and mapping profiles.
- `src/EShop.AppHost/` - Aspire orchestration for API, frontend, database, and supporting services.
- `src/EShop.BlobStorageInitializer/` - Initializes blob storage containers for local development.
- `src/EShop.Data.Tests/` - Data layer unit tests.
- `src/EShop.Data/` - EF Core DbContext, entity configurations, and migrations.
- `src/EShop.Domain.Tests/` - Domain layer unit tests.
- `src/EShop.Domain/` - Domain entities and business logic abstractions.
- `src/EShop.Gateway/` - YARP reverse proxy that fronts API calls.
- `src/EShop.ImagesQueueWorker/` - Background worker for image processing queue.
- `src/EShop.MigrationService/` - Service to apply database migrations during orchestration.
- `src/EShop.ServiceDefaults/` - Cross-cutting host defaults (observability, health checks, etc.).
- `src/EShop.Shared.Tests/` - Shared library unit tests.
- `src/EShop.Shared/` - Shared representations and helpers consumed across projects.
- `src/EShop.Web/` - Blazor Server frontend components, assets, and HTTP clients.
- `src/EShop.sln` - .NET solution tying together all projects.

## Quick Start

EShop uses .NET Aspire to orchestrate backend, frontend, database and all the 
other dependencies with a single command.

See the full guide here: [Quick Start](docs/quickstart.md)

## Blazor Frontend

The Blazor Server app in `src/EShop.Web/` uses MudBlazor for the UI layer, 
giving the catalog a modern component set, consistent theming, and responsive 
layouts while staying within the .NET stack.

## Catalog API

- [Error Handling](docs/api/errorhandling.md)
- [Azure Blob Storage](docs/azure-blob-storage.md)
- [Observability](docs/observability.md)

## EF Core migrations

See [EF Core migrations](docs/ef-core-migrations.md).

## Other Docs

- [Roadmap](docs/roadmap.md)
