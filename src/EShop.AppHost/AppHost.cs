var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.EShop_API>("api");

builder.Build().Run();
