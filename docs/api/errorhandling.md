# Error Handling

The API uses ASP.NET Core’s built-in ProblemDetails format for all errors.

## Exception → HTTP Status Mapping

The application maps exception types to HTTP status codes using .NET 9’s StatusCodeSelector:

```csharp
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        DomainException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    }
});
```

## DomainException

A DomainException is thrown when an entity already exists in the database.
It is translated into a 409 Conflict response.

```json
{
  "status": 409,
  "title": "Conflict",
  "detail": "Entity already exists.",
  "instance": "POST /api/products",
  "traceId": "00-...-..."
}
```

## ProblemDetails Enrichment

The service also enriches error responses with:

- requestId

- traceId

- instance (HTTP method + route)

These values help with troubleshooting and observability.