using OrderDispatcher.Core.Dtos;

namespace OrderDispatcher.Core.Contracts
{
    public record OrderReceivedEvent(
        Guid CorrelationId,
        string OrderId,
        OrderDto OrderData
    );
}