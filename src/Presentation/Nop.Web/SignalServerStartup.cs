using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace Nop.Web
{
    public class SignalServerStartup
    {
        private IWebServerData _webServerData;

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILogger<SignalServerStartup> logger)
        {
            _webServerData = app.ApplicationServices.GetRequiredService<IWebServerData>();

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                logger.LogInformation("Signal Server Stopped");
                _webServerData.SignalServerStopped.SetResult(true);
            });

            app.Run(async (context) =>
            {
                if (_webServerData.ServiceProvider != null)
                {
                    var pluginService = _webServerData.ServiceProvider.GetRequiredService<IPluginService>();
                    var plugin = pluginService.GetPluginDescriptorBySystemName<IPlugin>("Misc.PersonalCloud", LoadPluginsMode.InstalledOnly);
                    if (plugin != null)
                    {
                        await context.Response.WriteAsync("The PersonalCloud Plugin is installed.\r\n\r\n");
                    }
                    else
                    {
                        await context.Response.WriteAsync("The PersonalCloud Plugin is NOT installed.\r\n\r\n");
                    }

                    // Nop Services are accessible
                    var customerService = _webServerData.ServiceProvider.GetRequiredService<ICustomerService>();
                    foreach(var item in customerService.GetAllCustomers())
                    {
                        var name = item.Username ?? item.Email;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            await context.Response.WriteAsync(name + "\r\n");
                        }
                    }
                }
            });
        }
    }
}
