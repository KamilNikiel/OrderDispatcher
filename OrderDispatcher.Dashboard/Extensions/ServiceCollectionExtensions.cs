using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using OrderDispatcher.Core.Constants;
using OrderDispatcher.Core.Database;

namespace OrderDispatcher.Dashboard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string Scheme = "Basic";
        private const string OrderDispatcherPrefix = "order-dispatcher";

        public static IServiceCollection AddDashboardInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderDispatcherDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString(ConfigConstants.DefaultConnection)));

            services.AddHttpClient(ConfigConstants.RabbitMq, client =>
            {
                var rabbitHost = configuration[ConfigConstants.RabbitMqHost];
                var rabbitUser = configuration[ConfigConstants.RabbitMqUser];
                var rabbitPassword = configuration[ConfigConstants.RabbitMqPassword];
                var rabbitApiPort = configuration[ConfigConstants.RabbitMqApiPort];

                client.BaseAddress = new Uri($"http://{rabbitHost}:{rabbitApiPort}/api/");

                var authBytes = Encoding.ASCII.GetBytes($"{rabbitUser}:{rabbitPassword}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Scheme, Convert.ToBase64String(authBytes));
            });

            return services;
        }

        public static IServiceCollection AddDashboardMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: OrderDispatcherPrefix, includeNamespace: false));

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration[ConfigConstants.RabbitMqHost], "/", h =>
                    {
                        h.Username(configuration[ConfigConstants.RabbitMqUser]!);
                        h.Password(configuration[ConfigConstants.RabbitMqPassword]!);
                    });
                });
            });

            return services;
        }
    }
}