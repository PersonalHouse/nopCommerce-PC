using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace SignalServer.Core
{
    internal class ConfigureTokenValidationParameters : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly ISignalServerOptions _signalServerOptions;

        public ConfigureTokenValidationParameters(ISignalServerOptions signalServerOptions)
        {
            _signalServerOptions = signalServerOptions;
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            if (name == JwtBearerDefaults.AuthenticationScheme)
            {
                options.TokenValidationParameters.IssuerSigningKeyResolver = this.CreateResolver();
            }
        }

        private IssuerSigningKeyResolver CreateResolver()
        {
            return (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
                => new[] { new ECDsaSecurityKey(_signalServerOptions.SigningKey.GetECDsaPrivateKey()) };
        }
    }
}
