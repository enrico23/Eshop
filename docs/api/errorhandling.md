# Error Handling

The API returns errors using ASP.NET Core ProblemDetails responses.

## Exception to status mapping

Unhandled exceptions are mapped to HTTP status codes via `UseExceptionHandler`:

```csharp
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    }
});
```

## DomainException handling

`DomainException` is handled by a dedicated exception handler and returned as
`409 Conflict`:

```json
{
  "status": 409,
  "title": "Business rule violation.",
  "detail": "Entity already exists.",
  "type": "https://httpstatuses.com/409"
}
```

## DbUpdateConcurrencyException handling

`DbUpdateConcurrencyException` is handled by `ConcurrencyExceptionHandler` and
returned as `409 Conflict`:

```json
{
  "status": 409,
  "title": "Concurrency conflict.",
  "detail": "The resource was modified by another request. Refresh and retry.",
  "type": "https://httpstatuses.com/409"
}
```

## ProblemDetails enrichment

The API enriches ProblemDetails responses with:

- `requestId`
- `traceId`
- `instance` (HTTP method + route)

These values help with troubleshooting and observability.
