using EShop.Shared.Observability.Correlation;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();

var correlationOptions = new CorrelationIdOptions { HeaderName = "X-Correlation-ID" };

app.Use(async (context, next) =>
{
    string? incoming = null;
    if (context.Request.Headers.TryGetValue(correlationOptions.HeaderName, out var existing))
        incoming = existing.ToString();

    var cid = CorrelationIdValidator.IsValid(incoming, correlationOptions)
        ? incoming!
        : CorrelationIdGenerator.CreateId();

    if (Activity.Current?.IsAllDataRequested == true)
        Activity.Current.SetTag(correlationOptions.ActivityTagName, cid);

    context.Request.Headers[correlationOptions.HeaderName] = cid;

    context.Response.OnStarting(() =>
    {
        context.Response.Headers[correlationOptions.HeaderName] = cid;
        return Task.CompletedTask;
    });

    await next();
});

app.MapReverseProxy();
app.Run();
