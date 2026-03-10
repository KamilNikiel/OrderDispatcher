using Dapper;
using MassTransit;
using OrderDispatcher.Core.Contracts;
using OrderDispatcher.Core.Database;

namespace OrderDispatcher.Workers.Ingestion
{
    internal class SqlWarehouseWorker : BackgroundService
    {
        private readonly IBus _bus;
        private readonly IWarehouseDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<SqlWarehouseWorker> _logger;

        public SqlWarehouseWorker(IBus bus, IWarehouseDbConnectionFactory dbConnectionFactory, ILogger<SqlWarehouseWorker> logger)
        {
            _bus = bus;
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SQL Warehouse Worker started and polling.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var connection = _dbConnectionFactory.CreateConnection();

                    var query = @"
                        SELECT Id, OrderId, WarehouseDocumentId 
                        FROM MockWarehouseDocuments 
                        WHERE IsProcessed = 0";

                    var pendingDocs = await connection.QueryAsync<DocumentDto>(query);

                    foreach (var doc in pendingDocs)
                    {
                        _logger.LogInformation("Found pending warehouse document for Order {OrderId}.", doc.OrderId);

                        var warehouseEvent = new WarehouseDocumentIssuedEvent(
                            CorrelationId: Guid.NewGuid(),
                            OrderId: doc.OrderId,
                            WarehouseDocumentId: doc.WarehouseDocumentId
                        );

                        await _bus.Publish(warehouseEvent, stoppingToken);

                        await connection.ExecuteAsync(
                            "UPDATE MockWarehouseDocuments SET IsProcessed = 1 WHERE Id = @Id",
                            new { doc.Id });

                        _logger.LogInformation("Published WarehouseEvent and marked {OrderId} as processed.", doc.OrderId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling SQL database.");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        private class DocumentDto
        {
            public required int Id { get; set; }
            public required string OrderId { get; set; }
            public required string WarehouseDocumentId { get; set; }

            public DocumentDto() { }
        }
    }
}