using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Options;
using SignalServer.Core;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SignalServer
{
    public class GlobalData : ISignalServerOptions, IDisposable
    {
        private readonly SignalServerSettings _signalServerSettings;

        public X509Certificate2 SigningKey { get; }
        public int AccessTokenLifetime { get; }

        public GlobalData(IOptions<SignalServerSettings> options)
        {
            _signalServerSettings = options?.Value ?? throw new ArgumentException(nameof(options));
            this.AccessTokenLifetime = _signalServerSettings.AccessTokenLifetime;
            if (this.AccessTokenLifetime <= 0)
            {
                this.AccessTokenLifetime = SignalServerSettings.DEFAULT_ACCESS_TOKEN_LIFETIME;
            }

            try
            {
                this.SigningKey = LoadFromStoreCert(_signalServerSettings.SigningKey);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to load signing certificate.", ex);
            }
        }

        private X509Certificate2 LoadFromStoreCert(CertificateConfig certInfo)
        {
            var subject = certInfo.Subject;
            var storeName = string.IsNullOrEmpty(certInfo.Store) ? StoreName.My.ToString() : certInfo.Store;
            var location = certInfo.Location;
            var storeLocation = StoreLocation.CurrentUser;
            if (!string.IsNullOrEmpty(location))
            {
                storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), location, ignoreCase: true);
            }
            var allowInvalid = certInfo.AllowInvalid ?? false;

            return CertificateLoader.LoadFromStoreCert(subject, storeName, storeLocation, allowInvalid);
        }

        public void Dispose()
        {
            this.SigningKey?.Dispose();
        }
    }
}
