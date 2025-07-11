using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Security.Configuration;
using Security.Providers;
using System;

namespace Security.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Configuration from appsettings.json
        public static IServiceCollection AddSecretService(this IServiceCollection services, IConfiguration configuration, string sectionName = SecretServiceOptions.SectionName)
        {
            services.Configure<SecretServiceOptions>(configuration.GetSection(sectionName));
            services.AddSingleton<ISecretServiceProvider>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<SecretServiceOptions>>().Value;
                return CreateProvider(options);
            });
            services.AddSingleton<ISecrets>(provider =>
            {
                var serviceProvider = provider.GetRequiredService<ISecretServiceProvider>();
                return serviceProvider.CreateService();
            });

            return services;
        }

        // Manual configuration with action
        public static IServiceCollection AddSecretService(this IServiceCollection services, Action<SecretServiceOptions> configureOptions)
        {
            var options = new SecretServiceOptions();
            configureOptions(options);
            return services.AddSecretService(options);
        }

        // Manual configuration with options object
        public static IServiceCollection AddSecretService(this IServiceCollection services, SecretServiceOptions options)
        {
            services.AddSingleton<ISecretServiceProvider>(_ => CreateProvider(options));
            services.AddSingleton<ISecrets>(provider =>
            {
                var serviceProvider = provider.GetRequiredService<ISecretServiceProvider>();
                return serviceProvider.CreateService();
            });

            return services;
        }

        private static ISecretServiceProvider CreateProvider(SecretServiceOptions options)
        {
            return options.DefaultProvider switch
            {
                "SecretsManager" => new SecretsManagerProvider(options.SecretsManager),
                "ParamStore" => new ParamStoreProvider(options.ParamStore),
                _ => new ParamStoreProvider(options.ParamStore) // Default to ParamStore
            };
        }
    }
}