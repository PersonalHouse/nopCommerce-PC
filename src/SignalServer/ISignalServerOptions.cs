using System.Security.Cryptography.X509Certificates;

namespace SignalServer.Core
{
    public interface ISignalServerOptions
    {
        X509Certificate2 SigningKey { get; }

        int AccessTokenLifetime { get; }
    }
}
