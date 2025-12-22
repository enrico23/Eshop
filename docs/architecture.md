# Architecture Overview

## High-level flow

```mermaid
flowchart LR
    Browser[Browser] --> Web[EShop.Web]
    Web --> Gateway[EShop.Gateway (YARP)]
    Gateway --> Api[EShop.API]
    Api --> Db[(SQL Server)]
    Api --> Blob[(Azure Blob Storage)]
    Api --> Bus[(Azure Service Bus)]
    Bus --> Worker[EShop.ImagesQueueWorker]
```

1. The browser connects to `EShop.Web` (Blazor Server).
2. The UI calls the Catalog API through `EShop.Gateway` (YARP).
3. The Gateway attaches/validates `X-Correlation-ID` and forwards the request.
4. The API reads catalog data from SQL Server via EF Core.
5. Product images are stored in Azure Blob Storage and streamed by the API.
6. Image-related work is processed by `EShop.ImagesQueueWorker` via Service Bus.

## Services

- `EShop.AppHost`: Aspire orchestration for local development.
- `EShop.Web`: Blazor Server UI.
- `EShop.Gateway`: Reverse proxy and edge correlation ID handling.
- `EShop.API`: Catalog API, image endpoints, and business rules.
- `EShop.Data`: EF Core model and migrations.
- `EShop.BlobStorageInitializer`: Ensures blob container exists.
- `EShop.ImagesQueueWorker`: Background image processing.

## Observability

- Tracing and metrics are provided via OpenTelemetry defaults.
- Business correlation is carried via `X-Correlation-ID` and tagged as `business.request_id`.
