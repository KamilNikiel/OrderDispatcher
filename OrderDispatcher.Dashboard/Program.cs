using OrderDispatcher.Dashboard.Extensions;

namespace OrderDispatcher.Dashboard
{
    public class Program
    {
        private const string ErrorHandlingPath = "/Error";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();

            builder.Services.AddDashboardInfrastructure(builder.Configuration);
            builder.Services.AddDashboardMassTransit(builder.Configuration);

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(ErrorHandlingPath);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages().WithStaticAssets();

            app.Run();
        }
    }
}