var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.EShop_API>("api");

builder.Build().Run();
 