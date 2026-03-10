using MassTransit;
using OrderDispatcher.Core.Contracts;
using OrderDispatcher.Core.Interfaces;

namespace OrderDispatcher.Workers.Consumers
{
    internal class MigrateToProductionConsumer : IConsumer<MigrateToProductionCommand>
    {
        private readonly IProductionApiClient _apiClient;
        private readonly ILogger<MigrateToProductionConsumer> _logger;

        public MigrateToProductionConsumer(IProductionApiClient apiClient, ILogger<MigrateToProductionConsumer> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<MigrateToProductionCommand> context)
        {
            _logger.LogInformation("Executing migration to production for Saga: {CorrelationId}", context.Message.CorrelationId);

            var success = await _apiClient.SendOrderAsync($"{context.Message.OrderId}", $"{context.Message.WarehouseDocumentId}");

            if (!success)
            {
                throw new Exception("API failed to process the order. Sending to DLQ.");
            }
        }
    }
}