using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrderDispatcher.Core.Sagas;
using OrderDispatcher.Core.Database;
using OrderDispatcher.Core.Constants;
using System.Text.Json;

namespace OrderDispatcher.Dashboard.Pages
{
    internal class IndexModel : PageModel
    {
        private const string messages = "messages";

        private readonly OrderDispatcherDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public List<OrderDispatcherSagaState> AllOrders { get; set; } = new();
        public int DeadLetterMessageCount { get; set; } = 0;

        public int TotalOrders => AllOrders.Count;
        public int CompletedOrders => AllOrders.Count(o => o.CurrentState == SagaStateNames.Finished);
        public int PendingOrders => AllOrders.Count(o => o.CurrentState != SagaStateNames.Finished);

        public IndexModel(OrderDispatcherDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            AllOrders = await _dbContext.Set<OrderDispatcherSagaState>()
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            var client = _httpClientFactory.CreateClient(ConfigConstants.RabbitMq);
            try
            {
                // %2F Separates path segments
                var response = await client.GetAsync("queues/%2f/order-dispatcher-migrate-to-production_error");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    DeadLetterMessageCount = doc.RootElement.GetProperty(messages).GetInt32();
                }
            }
            catch
            {
                DeadLetterMessageCount = 0;
            }
        }
    }
}