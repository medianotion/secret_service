using Security.Configuration;

namespace Security.Providers
{
    public class ParamStoreProvider : ISecretServiceProvider
    {
        private readonly ParamStoreOptions _options;

        public ParamStoreProvider(ParamStoreOptions options)
        {
            _options = options ?? throw new System.ArgumentNullException(nameof(options));
        }

        public ISecrets CreateService()
        {
            return new ParamStore(_options);
        }
    }
}