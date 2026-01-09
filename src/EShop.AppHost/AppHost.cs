using Aspire.Keycloak.DeterministicHttps.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var disableFrontend = builder.Configuration.GetValue("DisableFrontend", false);
var dependenciesOnly = builder.Configuration.GetValue("DependenciesOnly", false);
var seedSampleData = builder.Environment.IsDevelopment();

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
if (seedSampleData)
    migrationWorkerService = migrationWorkerService.WithEnvironment("SeedSampleData", "true");

// SERVICE BUS
var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator(emulator =>
    {
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });
var serviceBusQueue = serviceBus.AddServiceBusQueue("images-queue");
serviceBus.AddServiceBusQueue("dead-letter-queue");

var imagesQueueWorker = builder.AddProject<Projects.EShop_ImagesQueueWorker>("ImagesQueueWorker")
    .WithReference(serviceBusQueue)
    .WaitFor(serviceBusQueue)
    .WithReference(azureBlobStorage)
    .WaitFor(azureBlobStorage);

// KEYCLOAK
var username = builder.AddParameter("keycloak-username");
var keyloakPassword = builder.AddParameter("keycloak-password", secret: true);
var keycloak = builder.AddKeycloakFixedHttpsPort("keycloak", httpPort: 8080, httpsPort: 8093, username, password)
    .WithDataVolume("keycloak");

// APPLICATION PROJECTS
if (!dependenciesOnly)
{
    // API CONFIGURATION
    var catalogApi = builder.AddProject<Projects.EShop_API>("CatalogApi")
           .WaitFor(db)
           .WithReference(db)
           .WaitFor(migrationWorkerService)
           .WaitFor(azureBlobStorage)
           .WithReference(azureBlobStorage)
           .WaitFor(serviceBusQueue)
           .WithReference(serviceBusQueue)
           .WithReference(keycloak);

    var gateway = builder.AddProject<Projects.EShop_Gateway>("Gateway")
        .WaitFor(catalogApi)
        .WithReference(catalogApi)
         .WithReference(keycloak);

    // FRONTEND
    if (!disableFrontend)
    {
        var frontend = builder.AddProject<Projects.EShop_Web>("Web")
            .WithExternalHttpEndpoints();
        frontend = frontend
            .WaitFor(gateway)
            .WithReference(gateway)
            .WithReference(keycloak);
    }
}

builder.Build().Run();
 