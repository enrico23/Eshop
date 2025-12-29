using EShop.ImagesQueueWorker;
using EShop.Shared.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("EShop.ImagesQueueWorker");
    });

builder.AddAzureServiceBusClient("images-queue");

var blobConnectionString = builder.Configuration.GetConnectionString("Blobs") 
    ?? throw new InvalidOperationException("Connection string for the blob storage is missing");

builder.Services.AddAzureBlobStorage(
     blobConnectionString!,
     options =>
     {
         builder.Configuration.GetSection("AzureBlobStorage").Bind(options);
     });

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
