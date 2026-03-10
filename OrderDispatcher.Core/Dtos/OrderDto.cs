namespace OrderDispatcher.Core.Dtos
{
    public record OrderDto(
        string OrderId,
        decimal TotalAmount,
        CustomerDto Customer,
        List<OrderItemDto> Items
    );
}