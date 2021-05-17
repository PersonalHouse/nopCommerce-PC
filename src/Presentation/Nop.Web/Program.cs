using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nop.Services.Plugins;

namespace Nop.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var mainWebHost = CreateHostBuilder(args).Build();
            var webServerData = mainWebHost.Services.GetRequiredService<IWebServerData>(); // Some services are still not available
            var mainWebTask = mainWebHost.RunAsync();
            await webServerData.MainWebStarted.Task;
            var logger = webServerData.ServiceProvider.GetService<ILogger<Startup>>();
            var pluginService = webServerData.ServiceProvider.GetRequiredService<IPluginService>();
            var plugin = pluginService.GetPluginDescriptorBySystemName<IPlugin>("Misc.PersonalCloud", LoadPluginsMode.InstalledOnly);
            if (plugin != null)
            {
                logger?.LogInformation("Start Signal Server");
                var signalServerHost = CreateSignalServerHostBuilder(webServerData).Build();
                var signalServerTask = signalServerHost.RunAsync();
                await Task.WhenAny(
                    mainWebTask,
                    signalServerTask
                );
            }
            else
            {
                webServerData.SignalServerStopped.SetResult(true);
                await mainWebTask;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<MyHostedService>(); // Waiting for SignalServer stopped
                });
        }

        public static IHostBuilder CreateSignalServerHostBuilder(IWebServerData webServerData)
        {
            return Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseUrls("http://*:5002", "https://*:5003")
                        .UseStartup<SignalServerStartup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(webServerData);
                });
        }
    }

    public class MyHostedService : IHostedService
    {
        private readonly ILogger<MyHostedService> _logger;
        private readonly IWebServerData _webServerData;

        public MyHostedService(ILogger<MyHostedService> logger, IWebServerData webServerData)
        {
            _logger = logger;
            _webServerData = webServerData;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            await _webServerData.SignalServerStopped.Task;
        }
    }
}
