using OrderDispatcher.Workers.Extensions;

namespace OrderDispatcher.Workers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddWorkersInfrastructure(builder.Configuration, builder.Environment);
            builder.Services.AddWorkersMassTransit(builder.Configuration);

            var host = builder.Build();

            host.InitializeDatabases();

            host.Run();
        }
    }
}