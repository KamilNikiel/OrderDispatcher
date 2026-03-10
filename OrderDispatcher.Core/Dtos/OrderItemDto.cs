namespace OrderDispatcher.Core.Dtos
{
    public record OrderItemDto(
        string Sku,
        int Quantity,
        decimal UnitPrice
    );
}