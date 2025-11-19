var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.EShop_API>("api");

builder.AddProject<Projects.Eshop_Web>("blazor").WaitFor(api);

builder.Build().Run();
 