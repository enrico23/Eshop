using EShop.ImagesQueueWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureServiceBusClient("images-queue");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
