namespace OrderDispatcher.Core.Domain
{
    public class OrderItem
    {
        public string Sku { get; private set; } = null!;
        public int Quantity { get; private set; }

        private OrderItem() { }

        public OrderItem(string sku, int quantity)
        {
            Sku = sku;
            Quantity = quantity;
        }
    }
}