using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();

app.Use(async (context, next) =>
{
    const string HeaderName = "X-Correlation-ID";

    var cid = context.Request.Headers.TryGetValue(HeaderName, out var existing)
        && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : (Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));

    context.Request.Headers[HeaderName] = cid;

    context.Response.OnStarting(() =>
    {
        context.Response.Headers[HeaderName] = cid;
        return Task.CompletedTask;
    });

    await next();
});

app.MapReverseProxy();
app.Run();
