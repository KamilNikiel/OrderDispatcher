using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrderDispatcher.Core.Constants;
using OrderDispatcher.Core.Database;
using RabbitMQ.Client;
using System.Text.Json;

namespace OrderDispatcher.Dashboard.Pages
{
    internal class IndexModel : PageModel
    {
        private const string messages = "messages";
        private const string ErrorQueue = "order-dispatcher-migrate-to-production_error";
        private const string MainQueue = "order-dispatcher-migrate-to-production";

        private readonly OrderDispatcherDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public List<OrderDispatcherSagaState> AllOrders { get; set; } = new();
        public int DeadLetterMessageCount { get; set; } = 0;

        public int TotalOrders => AllOrders.Count;
        public int CompletedOrders => AllOrders.Count(o => o.CurrentState == SagaStateNames.Final);
        public int PendingOrders => AllOrders.Count(o => o.CurrentState != SagaStateNames.Final);

        public IndexModel(OrderDispatcherDbContext dbContext, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
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
                var response = await client.GetAsync($"queues/%2f/{ErrorQueue}");

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

        public async Task<IActionResult> OnPostResendAllAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration[ConfigConstants.RabbitMqHost]!,
                UserName = _configuration[ConfigConstants.RabbitMqUser]!,
                Password = _configuration[ConfigConstants.RabbitMqPassword]!
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            while (true)
            {
                var result = await channel.BasicGetAsync(ErrorQueue, autoAck: false);

                if (result == null) break;

                var properties = new BasicProperties
                {
                    Headers = result.BasicProperties.Headers,
                    ContentType = result.BasicProperties.ContentType,
                    MessageId = result.BasicProperties.MessageId,
                    CorrelationId = result.BasicProperties.CorrelationId,
                    DeliveryMode = result.BasicProperties.DeliveryMode
                };

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: MainQueue,
                    mandatory: false,
                    basicProperties: properties,
                    body: result.Body);

                await channel.BasicAckAsync(result.DeliveryTag, multiple: false);
            }

            return RedirectToPage();
        }
    }
}