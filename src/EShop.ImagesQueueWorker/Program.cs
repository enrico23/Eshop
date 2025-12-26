using Azure.Storage.Blobs;
using EShop.ImagesQueueWorker;
using EShop.Shared.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureServiceBusClient("images-queue");

var blobConnectionString = builder.Configuration.GetConnectionString("Blobs");
builder.Services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var options = configuration
        .GetSection("AzureBlobStorage")
        .Get<ImageStorageServiceOptions>();

    return options is null
        ? throw new InvalidOperationException(
            "ImageStorageServiceOptions configuration section is missing or invalid.")
        : options;
});
builder.Services.AddSingleton(sp =>
{
    var serviceClient = sp.GetRequiredService<BlobServiceClient>();
    return serviceClient.GetBlobContainerClient("product-images");
});
builder.Services.AddSingleton<IImageStorageService, ImageStorageService>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
