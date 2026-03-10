using FluentFTP;
using OrderDispatcher.Core.Contracts;
using OrderDispatcher.Core.Dtos;
using OrderDispatcher.Core.Constants;
using MassTransit;
using System.Text.Json;

namespace OrderDispatcher.Workers.Ingestion
{
    internal class FtpIngestionWorker : BackgroundService
    {
        private readonly IBus _bus;
        private readonly IConfiguration _config;
        private readonly ILogger<FtpIngestionWorker> _logger;

        public FtpIngestionWorker(IBus bus, IConfiguration config, ILogger<FtpIngestionWorker> logger)
        {
            _bus = bus;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FTP Ingestion Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var ftpClient = new AsyncFtpClient(
                        _config[ConfigConstants.FtpHost],
                        _config[ConfigConstants.FtpUser],
                        _config[ConfigConstants.FtpPassword]);

                    await ftpClient.AutoConnect(stoppingToken);

                    var files = await ftpClient.GetListing($"/{ConfigConstants.FtpNewFolder}", stoppingToken);

                    foreach (var file in files.Where(f => f.Type == FtpObjectType.File))
                    {
                        if (DateTime.UtcNow - file.Modified < TimeSpan.FromSeconds(2))
                        {
                            continue;
                        }

                        _logger.LogInformation("Downloading and processing order file: {FileName}", file.Name);

                        var jsonBytes = await ftpClient.DownloadBytes(file.FullName, stoppingToken);
                        var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);

                        try
                        {
                            var orderDto = JsonSerializer.Deserialize<OrderDto>(jsonString);

                            if (orderDto == null || string.IsNullOrEmpty(orderDto.OrderId))
                            {
                                throw new Exception("JSON deserialization resulted in a null or invalid order.");
                            }

                            await _bus.Publish(new OrderReceivedEvent(
                                Guid.NewGuid(),
                                orderDto.OrderId,
                                orderDto
                            ), stoppingToken);

                            await ftpClient.MoveFile(file.FullName, $"/{ConfigConstants.FtpArchiveFolder}/{file.Name}", token: stoppingToken);

                            _logger.LogInformation("Order {OrderId} published and archived.", orderDto.OrderId);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to parse JSON for file {FileName}. Moving to error folder.", file.Name);

                            await ftpClient.MoveFile(file.FullName, $"/{ConfigConstants.FtpErrorFolder}/{file.Name}", token: stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while polling the FTP server.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}