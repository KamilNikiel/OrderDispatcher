namespace OrderDispatcher.Core.Domain
{
    public class Order
    {
        public string OrderId { get; private set; } = null!;
        public decimal TotalAmount { get; private set; }
        public Customer Customer { get; private set; } = null!;

        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        private Order() { }

        public Order(string orderId, decimal totalAmount, Customer customer, IEnumerable<OrderItem> items)
        {
            OrderId = orderId;
            TotalAmount = totalAmount;
            Customer = customer;
            _items.AddRange(items);
        }
    }
}