using OrderDispatcher.Core.Dtos;

namespace OrderDispatcher.Core.Contracts
{
    public record MigrateToProductionCommand(
        Guid CorrelationId,
        string OrderId,
        string WarehouseDocumentId,
        OrderDto OrderData);
}