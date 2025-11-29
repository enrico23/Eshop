using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var disableFrontend = builder.Configuration.GetValue("DisableFrontend", false);

// SQL SERVER & DB SETUP
var databaseName = "CatalogDb";
var password = builder.AddParameter("password", secret: true);
var sql = builder.AddSqlServer("sql", port:14347, password: password)
    .WithLifetime(ContainerLifetime.Persistent); ;
var db = sql.AddDatabase(databaseName);

// EF MIGRATIONS
var migrationWorkerService = builder.AddProject<Projects.EShop_MigrationService>("migration")
    .WithReference(db)
    .WaitFor(db);

// API CONFIGURATION
var catalogApi = builder.AddProject<Projects.EShop_API>("api")
       .WithReference(db)
       .WaitFor(migrationWorkerService);

// FRONTEND
IResourceBuilder<ProjectResource>? frontend = null!;
if (!disableFrontend)
{
    frontend = builder.AddProject<Projects.EShop_Web>("web").WithExternalHttpEndpoints();
    frontend = frontend
        .WithReference(catalogApi)
        .WaitFor(catalogApi);
}

builder.Build().Run();
 