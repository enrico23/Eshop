using Microsoft.Data.SqlClient;

public sealed class WebsiteDbMigratorService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
     ILogger<WebsiteDbMigratorService> logger) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Migrating data into WebsiteDb...");
            using var scope = serviceProvider.CreateScope();
            SqlConnection? sqlConnection = scope.ServiceProvider.GetService<SqlConnection>() 
                ?? throw new InvalidOperationException("Sql Connection not valid.");
            WebsiteDbInitializer websiteDbInitializer = new(sqlConnection);
            await websiteDbInitializer.InitializeAsync(stoppingToken);
            logger.LogInformation("Migrating data into WebsiteDb completed.");
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }
}

public class WebsiteDbInitializer(SqlConnection connection)
{
    public async Task InitializeAsync(CancellationToken stoppingToken)
    {
        await connection.OpenAsync(stoppingToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
IF OBJECT_ID(N'dbo.Websites', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Websites
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Url NVARCHAR(1024) NOT NULL,
        XPathExpression NVARCHAR(1024) NULL
    );
END";

        await command.ExecuteNonQueryAsync(stoppingToken);
    }
}