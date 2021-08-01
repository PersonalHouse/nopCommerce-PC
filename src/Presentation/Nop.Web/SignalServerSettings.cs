namespace SignalServer
{
    public class SignalServerSettings
    {
        public CertificateConfig SigningKey { get; set; }

        public int AccessTokenLifetime { get; set; }

        public const int DEFAULT_ACCESS_TOKEN_LIFETIME = 3600 * 24;
    }

    public class CertificateConfig
    {
        public string Subject { get; set; }

        public string Store { get; set; }

        public string Location { get; set; }

        public bool? AllowInvalid { get; set; }
    }
}
