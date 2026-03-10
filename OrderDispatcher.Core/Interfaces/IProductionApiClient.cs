using System.Threading.Tasks;

namespace OrderDispatcher.Core.Interfaces
{
    public interface IProductionApiClient
    {
        Task<bool> SendOrderAsync(string orderId, string warehouseDocumentId);
    }
}