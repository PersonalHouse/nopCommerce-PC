using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SignalServer;
using SignalServer.Core;
using SignalServer.Devices;
using SignalServer.Users;

namespace Nop.Web
{
    public class SignalServerStartup
    {
        private IWebServerData _webServerData;

        public SignalServerStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SignalServerSettings>(Configuration.GetSection("SignalServer"));
            services.AddSingleton<GlobalData>();
            services.AddSingleton<ISignalServerOptions>(x => x.GetRequiredService<GlobalData>());

            services.AddSingleton((x) => _webServerData.ServiceProvider.GetRequiredService<IUserManager>());
            services.AddSingleton((x) => _webServerData.ServiceProvider.GetRequiredService<IDeviceManager>());

            services.AddSignalServerServices();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Signal Server", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<SignalServerStartup> logger)
        {
            _webServerData = app.ApplicationServices.GetRequiredService<IWebServerData>();

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                logger.LogInformation("Signal Server Stopped");
                _webServerData.SignalServerStopped.SetResult(true);
            });

            GlobalData globalData = serviceProvider.GetRequiredService<GlobalData>();

            var sb = new StringBuilder();
            sb.AppendLine("Load Signing Key successfully.");
            sb.AppendLine();
            sb.Append(globalData.SigningKey.ToString());
            logger?.LogInformation(sb.ToString());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Signal Server v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSignalServer();
        }
    }
}
