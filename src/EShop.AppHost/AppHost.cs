var builder = DistributedApplication.CreateBuilder(args);

var catalogApi = builder.AddProject<Projects.EShop_API>("api");

var blazor = builder.AddProject<Projects.EShop_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(catalogApi)
    .WaitFor(catalogApi);

builder.Build().Run();
 