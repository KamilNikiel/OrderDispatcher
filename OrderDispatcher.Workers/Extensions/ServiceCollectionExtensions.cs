using Dapper;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderDispatcher.Core.Constants;
using OrderDispatcher.Core.Database;
using OrderDispatcher.Core.Interfaces;
using OrderDispatcher.Core.Sagas;
using OrderDispatcher.Mocks.Api;
using OrderDispatcher.Mocks.Ftp;
using OrderDispatcher.Workers.Consumers;

namespace OrderDispatcher.Workers.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        private const string AppPath = "/app";
        private const string OrderDispatcherPrefix = "order-dispatcher";

        public static IServiceCollection AddWorkersInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddSingleton<IWarehouseDbConnectionFactory, WarehouseDbConnectionFactory>();

            services.AddDbContext<OrderDispatcherDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString(ConfigConstants.DefaultConnection)));

            services.AddHostedService<Ingestion.FtpIngestionWorker>();
            services.AddHostedService<Ingestion.SqlWarehouseWorker>();

            if (environment.IsDevelopment())
            {
                var folderName = configuration[ConfigConstants.FtpServerName];
                if (string.IsNullOrEmpty(folderName))
                {
                    throw new Exception($"Missing configuration for {ConfigConstants.FtpServerName}");
                }

                var ftpRoot = Path.Combine(AppPath, folderName);

                if (!Directory.Exists(ftpRoot)) Directory.CreateDirectory(ftpRoot);

                services.AddLocalFtpSimulation(ftpRoot);
                services.AddSingleton<IProductionApiClient, MockProductionApiClient>();
            }

            return services;
        }

        public static IServiceCollection AddWorkersMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: OrderDispatcherPrefix, includeNamespace: false));

                x.AddConsumer<MigrateToProductionConsumer>();

                x.AddSagaStateMachine<OrderDispatcherStateMachine, OrderDispatcherSagaState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<OrderDispatcherDbContext>();
                        r.UseSqlServer();
                    });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration[ConfigConstants.RabbitMqHost], "/", h =>
                    {
                        h.Username(configuration[ConfigConstants.RabbitMqUser]!);
                        h.Password(configuration[ConfigConstants.RabbitMqPassword]!);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Intervals(
                            TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(15),
                            TimeSpan.FromMinutes(30)
                        );
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}