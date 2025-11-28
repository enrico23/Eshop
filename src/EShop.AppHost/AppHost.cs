var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.EShop_API>("api");

var blazor = builder.AddProject<Projects.EShop_Web>("web");

builder.Build().Run();
 