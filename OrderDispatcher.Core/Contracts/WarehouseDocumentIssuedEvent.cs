namespace OrderDispatcher.Core.Contracts
{
    public record WarehouseDocumentIssuedEvent(
        Guid CorrelationId,
        string OrderId,
        string WarehouseDocumentId
    );
}