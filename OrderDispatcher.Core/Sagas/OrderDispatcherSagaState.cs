using MassTransit;
using OrderDispatcher.Core.Dtos;

public class OrderDispatcherSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string? WarehouseDocumentId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public OrderDto? OrderData { get; set; }
}