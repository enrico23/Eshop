# Full-Stack .NET E-Commerce shop

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

## Achitecture

[Architecture Overview](docs/architecture.md)

## Features

- Product catalog with categories and images
- Gateway-mediated API access via YARP
- Image storage in Azure Blob Storage with background processing
- SQL Server + EF Core with migrations
- Blazor Server UI for the catalog experience
- Orchestrated services via .NET Aspire with observability defaults

## Quick Start

EShop uses .NET Aspire to orchestrate backend, frontend, and database with a single command.

See the full guide here: [Quick Start](docs/quickstart.md)

## Blazor Frontend
The Blazor Server app in src/EShop.Web/ uses 
MudBlazor for the UI layer, giving the 
catalog a modern component set, consistent 
theming, and responsive layouts while 
staying within the .NET stack.

## Catalog API

- [EF Core Migrations](docs/api/efcore.md)
- [Error Handling](docs/api/errorhandling.md)
- [Azure Blob Storage](docs/azure-blob-storage.md)
- [Observability](docs/observability.md


## Authentication & Authorization

Authentication and authorization are handled using an external identity provider
and enforced at the application boundaries.

The configuration is documented in two layers:

- **Identity Provider configuration**
  - [`Keycloak`](docs/security/keycloak/index.md)
- **Application integration**
  - [`Security`](docs/security/index.md)


## Other Docs

- [Roadmap](docs/roadmap.md)
- [Scripts & Utilities](docs/scripts/index.md)