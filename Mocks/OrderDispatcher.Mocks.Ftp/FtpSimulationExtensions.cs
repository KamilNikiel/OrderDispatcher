using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using OrderDispatcher.Core.Constants;

namespace OrderDispatcher.Mocks.Ftp
{
    public static class FtpSimulationExtensions
    {
        public static IServiceCollection AddLocalFtpSimulation(this IServiceCollection services, string rootPath)
        {
            Directory.CreateDirectory(Path.Combine(rootPath, ConfigConstants.FtpNewFolder));
            Directory.CreateDirectory(Path.Combine(rootPath, ConfigConstants.FtpArchiveFolder));
            Directory.CreateDirectory(Path.Combine(rootPath, ConfigConstants.FtpErrorFolder));

            services.AddSingleton<IMembershipProvider, GuestMembershipProvider>();

            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem()
            );

            services.Configure<DotNetFileSystemOptions>(options =>
            {
                options.RootPath = rootPath;
            });

            services.AddHostedService<FtpServerBackgroundService>();

            return services;
        }
    }
}