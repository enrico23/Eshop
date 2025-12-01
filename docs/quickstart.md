## Running the demo

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [.NET Aspire](https://aspire.dev/get-started/prerequisites/)
- Container runtime: [Docker Desktop](https://www.docker.com/products/docker-desktop/) **or** [Rancher Desktop](https://rancherdesktop.io/)

### Full stack (API + frontend + database)

The default configuration starts all services via the Aspire AppHost.

```
cd src/EShop.AppHost

dotnet run
```

This will start:
- Start SQL Server + CatalogDb
- Run the migration service
- Start the Catalog API
- Start the Blazor frontend (EShop.Web)

Expose endpoints in the Aspire dashboard

### Dependencies only

When working on the API, it's often useful to run it directly from your IDE
while Aspire only runs the dependencies.

You can run only the dependecies in the AppHost using the `DependenciesOnly` flag:

```
cd src/EShop.AppHost

dotnet run -- DependenciesOnly=true
```

This will start:
- Start SQL Server + CatalogDb
- Run the migration service


### Backend only (API + database, no frontend)

When working on the Blazor frontend, it's often useful to run it directly from your IDE
while Aspire only runs the backend infrastructure.

You can disable the frontend in the AppHost using the `DisableFrontend` flag:

```
cd src/EShop.AppHost

dotnet run -- DisableFrontend=true
```

This will start:
- Start SQL Server + CatalogDb
- Run the migration service
- Start the Catalog API
