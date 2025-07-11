using System.Collections.Generic;

namespace Security.Configuration
{
    public class SecretServiceOptions
    {
        public const string SectionName = "SecretService";
        
        public string DefaultProvider { get; set; } = "ParamStore";
        public Dictionary<string, ProviderOptions> Providers { get; set; } = new Dictionary<string, ProviderOptions>();
        public ParamStoreOptions ParamStore { get; set; } = new ParamStoreOptions();
        public SecretsManagerOptions SecretsManager { get; set; } = new SecretsManagerOptions();
    }

    public class ProviderOptions
    {
        public string Type { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// AWS Parameter Store configuration options
    /// </summary>
    public class ParamStoreOptions : AwsConnectionOptions
    {
        // Inherits Region, Credentials, Retry, and Properties from AwsConnectionOptions
    }

    /// <summary>
    /// AWS Secrets Manager configuration options
    /// </summary>
    public class SecretsManagerOptions : AwsConnectionOptions
    {
        // Inherits Region, Credentials, Retry, and Properties from AwsConnectionOptions
    }
}