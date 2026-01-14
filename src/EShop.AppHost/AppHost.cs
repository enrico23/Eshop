using Aspire.Hosting;
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
var keycloakUsername = builder.AddParameter("keycloak-username", "admin");
var keycloakPassword = builder.AddParameter("keycloak-password", "admin");
var keycloak = builder.AddKeycloak("keycloak", port: 8080, keycloakUsername, keycloakPassword)
    .WithDataVolume()
    .WithOtlpExporter();
var realmConfigPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "realm-export.json");
var realmConfig = File.Exists(realmConfigPath);
if (realmConfig)
    keycloak.WithRealmImport(realmConfigPath);

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
           .WaitFor(keycloak)
           .WithReference(keycloak);

    var gateway = builder.AddProject<Projects.EShop_Gateway>("Gateway")
        .WaitFor(catalogApi)
        .WithReference(catalogApi);

    // FRONTEND
    if (!disableFrontend)
    {
        var frontend = builder.AddProject<Projects.EShop_Web>("Web")
            .WithExternalHttpEndpoints();
        frontend = frontend
            .WaitFor(gateway)
            .WithReference(gateway)
            .WaitFor(keycloak)
            .WithReference(keycloak);
    }
}

builder.Build().Run();
 