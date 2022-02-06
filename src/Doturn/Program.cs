using Doturn.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Doturn
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureLogging((hostContext, logging) =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "yyyy/MM/dd hh:mm:ss.fff ";
            });
            logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddSingleton<IPortAllocator, PortAllocator>();
            services.AddHostedService<StunServerService.StunServerService>();
            services.Configure<AppSettings>(hostContext.Configuration);
        });
    }
}