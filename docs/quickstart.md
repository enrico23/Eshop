## Running the demo

### Prerequisites

- Windows 11 only: install WSL before installing Docker Desktop or Rancher Desktop.
  - Command: `wsl --install` (see [WSL install docs](https://learn.microsoft.com/windows/wsl/install)).
  - Reboot after the install if prompted.
  - If WSL reports virtualization is disabled, enable it in BIOS/UEFI.
- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [.NET Aspire](https://aspire.dev/get-started/prerequisites/)
- Container runtime: [Docker Desktop](https://www.docker.com/products/docker-desktop/) **or** [Rancher Desktop](https://rancherdesktop.io/)

The AppHost provisions local dependencies (SQL Server, Azurite, and Service Bus) automatically.

### Full stack (API + frontend + database)

The default configuration starts all services via the Aspire AppHost.

```
cd src/EShop.AppHost

dotnet run
```

This will start:
- Start SQL Server + CatalogDb
- Run the migration service
- Initialize Blob Storage containers
- Start the Catalog API
- Start the Gateway (YARP)
- Start the Blazor frontend (EShop.Web)
- Start the images queue worker

Use the Aspire dashboard to inspect endpoints and traces.

### Dependencies only

When working on the API, it's often useful to run it directly from your IDE
while Aspire only runs the dependencies.

You can run only the dependencies in the AppHost using the `DependenciesOnly` flag:

```
cd src/EShop.AppHost

dotnet run -- DependenciesOnly=true
```

This will start:
- Start SQL Server + CatalogDb
- Run the migration service
- Initialize Blob Storage containers
- Start the Service Bus emulator


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
- Start the Gateway (YARP)
