using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var disableFrontend = builder.Configuration.GetValue("DisableFrontend", false);
var dependenciesOnly = builder.Configuration.GetValue("DependenciesOnly", false);

// AZURE BLOB STORAGE
var azureBlobStorage = builder.AddAzureStorage("AzureBlobStorageEmulator")
  .RunAsEmulator(azurite =>
  {
      azurite.WithLifetime(ContainerLifetime.Persistent);
      azurite.WithDataVolume();
  }).AddBlobs("Blobs");
var blobStorageInitializer = builder.AddProject<Projects.EShop_BlobStorageInitializer>("BlobStorageInitializer")
    .WithReference(azureBlobStorage)
    .WaitFor(azureBlobStorage);

// SQL SERVER & DB SETUP
var databaseName = "CatalogDb";
var password = builder.AddParameter("password", secret: true);
var sql = builder.AddSqlServer("SqlServerInstance", port:14347, password: password)
    .WithLifetime(ContainerLifetime.Persistent); ;
var db = sql.AddDatabase(databaseName);

// EF MIGRATIONS
var migrationWorkerService = builder.AddProject<Projects.EShop_MigrationService>("EfCoreMigratorJob")
    .WithReference(db)
    .WaitFor(db);

if (!dependenciesOnly)
{
    // API CONFIGURATION
    var catalogApi = builder.AddProject<Projects.EShop_API>("CatalogApi")
           .WithReference(db)
           .WaitFor(db)
            .WaitFor(migrationWorkerService)
           .WithReference(azureBlobStorage);

    // FRONTEND
    if (!disableFrontend)
    {
        var frontend = builder.AddProject<Projects.EShop_Web>("Eshop")
            .WithExternalHttpEndpoints();
        frontend = frontend
            .WithReference(catalogApi)
            .WaitFor(catalogApi);
    }
}

builder.Build().Run();
 