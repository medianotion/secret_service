using Security.Configuration;

namespace Security.Providers
{
    public class SecretsManagerProvider : ISecretServiceProvider
    {
        private readonly SecretsManagerOptions _options;

        public SecretsManagerProvider(SecretsManagerOptions options)
        {
            _options = options ?? throw new System.ArgumentNullException(nameof(options));
        }

        public ISecrets CreateService()
        {
            return new SecretsManager(_options);
        }
    }
}