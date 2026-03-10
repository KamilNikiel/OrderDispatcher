using FubarDev.FtpServer;
using Microsoft.Extensions.Hosting;

namespace OrderDispatcher.Mocks.Ftp
{
    public class FtpServerBackgroundService(IFtpServerHost ftpServerHost) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => ftpServerHost.StartAsync(stoppingToken);

        public override Task StopAsync(CancellationToken cancellationToken)
            => ftpServerHost.StopAsync(cancellationToken);
    }
}