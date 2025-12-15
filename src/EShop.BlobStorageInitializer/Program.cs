using Azure.Storage.Blobs;
using EShop.BlobStorageInitializer;

var builder = Host.CreateApplicationBuilder(args);

var blobConn = builder.Configuration.GetConnectionString("Blobs");
builder.Services.AddSingleton(_ => new BlobServiceClient(blobConn));

builder.Services.AddHostedService<BlobInitializerService>();

var host = builder.Build();
host.Run();
