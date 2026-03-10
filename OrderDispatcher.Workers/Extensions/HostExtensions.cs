using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderDispatcher.Core.Constants;
using OrderDispatcher.Core.Database;

namespace OrderDispatcher.Workers.Extensions
{
    internal static class HostExtensions
    {
        private const string MockWarehouseTable = "MockWarehouseDocuments";
        private const string Master = "master";

        public static void InitializeDatabases(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var provider = scope.ServiceProvider;

            var db = provider.GetRequiredService<OrderDispatcherDbContext>();
            db.Database.Migrate();

            InitializeWarehouseDb(provider);
        }

        private static void InitializeWarehouseDb(IServiceProvider provider)
        {
            CreateDatabase(provider);
            CreateTable(provider);
        }

        private static void CreateDatabase(IServiceProvider provider)
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var warehouseConnString = config.GetConnectionString(ConfigConstants.WarehouseDb)
                ?? throw new Exception("Warehouse connection string missing");

            var masterBuilder = new SqlConnectionStringBuilder(warehouseConnString) { InitialCatalog = Master };
            var targetDbName = new SqlConnectionStringBuilder(warehouseConnString).InitialCatalog;

            using (var masterConnection = new SqlConnection(masterBuilder.ConnectionString))
            {
                var dbExists = masterConnection.QueryFirstOrDefault<int>(
                    $"SELECT 1 FROM sys.databases WHERE name = '{targetDbName}'");

                if (dbExists == 0)
                {
                    masterConnection.Execute($"CREATE DATABASE [{targetDbName}]");
                }
            }
        }

        private static void CreateTable(IServiceProvider provider)
        {
            var warehouseDbFactory = provider.GetRequiredService<IWarehouseDbConnectionFactory>();
            using (var warehouseConnection = warehouseDbFactory.CreateConnection())
            {
                var tableExists = warehouseConnection.QueryFirstOrDefault<int>(
                    $"SELECT 1 FROM sys.tables WHERE name = '{MockWarehouseTable}'");

                if (tableExists == 0)
                {
                    warehouseConnection.Execute($@"
                        CREATE TABLE [dbo].[{MockWarehouseTable}] (
                            [Id] INT IDENTITY(1,1) PRIMARY KEY,
                            [OrderId] NVARCHAR(128) NOT NULL,
                            [WarehouseDocumentId] NVARCHAR(128) NOT NULL,
                            [IsProcessed] BIT NOT NULL DEFAULT 0,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                        )");
                }
            }
        }

    }
}