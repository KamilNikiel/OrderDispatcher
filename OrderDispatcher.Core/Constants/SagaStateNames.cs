namespace OrderDispatcher.Core.Constants
{
    public static class SagaStateNames
    {
        public const string Initial = "Initial";
        public const string WaitingForWarehouseDoc = "WaitingForWarehouseDoc";
        public const string WaitingForOrder = "WaitingForOrder";
        public const string Final = "Final";
    }
}