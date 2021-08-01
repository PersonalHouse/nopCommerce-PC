using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;

namespace SignalServer.Core
{
    public static class SignalServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSignalServer(this IApplicationBuilder builder)
        {
            builder.UseAuthentication();
            builder.UseAuthorization();

            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<EventHub>("/events", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                });
            });
            return builder;
        }
    }
}
