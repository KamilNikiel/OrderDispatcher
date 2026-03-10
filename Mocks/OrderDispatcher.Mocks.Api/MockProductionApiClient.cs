using Microsoft.Extensions.Logging;
using OrderDispatcher.Core.Interfaces;

namespace OrderDispatcher.Mocks.Api
{
    public class MockProductionApiClient : IProductionApiClient
    {
        private readonly ILogger<MockProductionApiClient> _logger;

        public MockProductionApiClient(ILogger<MockProductionApiClient> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendOrderAsync(string orderId, string warehouseDocumentId)
        {
            _logger.LogWarning("MOCK API CALL STARTING: Sending Order {OrderId} with Doc {DocId} to external system.", orderId, warehouseDocumentId);
            
            // Do work

            _logger.LogWarning("MOCK API CALL SUCCESS: External system accepted Order {OrderId}.", orderId);

            return true;
        }
    }
}