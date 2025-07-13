using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Security;
using Security.Extensions;
using Security.Configuration;
using Security.Providers;
using System;
using System.Collections.Generic;

namespace unittest
{
    public class DIIntegrationTests
    {
        [Fact]
        public void AddSecretService_WithActionConfiguration_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddSecretService(options =>
            {
                options.DefaultProvider = "ParamStore";
                options.ParamStore.Region = "us-west-2";
                options.ParamStore.Credentials.AuthenticationType = "AccessKey";
                options.ParamStore.Credentials.AccessKey = "test-key";
                options.ParamStore.Credentials.SecretKey = "test-secret";
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var secretProvider = serviceProvider.GetService<ISecretServiceProvider>();
            Assert.NotNull(secretProvider);
            Assert.IsType<ParamStoreProvider>(secretProvider);

            var secrets = serviceProvider.GetService<ISecrets>();
            Assert.NotNull(secrets);
            Assert.IsType<ParamStore>(secrets);
        }

        [Fact]
        public void AddSecretService_WithSecretsManagerProvider_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddSecretService(options =>
            {
                options.DefaultProvider = "SecretsManager";
                options.SecretsManager.Region = "eu-west-1";
                options.SecretsManager.Credentials.AuthenticationType = "STS";
                options.SecretsManager.Credentials.AccessKey = "test-key";
                options.SecretsManager.Credentials.SecretKey = "test-secret";
                options.SecretsManager.Credentials.SessionToken = "test-token";
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var secretProvider = serviceProvider.GetService<ISecretServiceProvider>();
            Assert.NotNull(secretProvider);
            Assert.IsType<SecretsManagerProvider>(secretProvider);

            var secrets = serviceProvider.GetService<ISecrets>();
            Assert.NotNull(secrets);
            Assert.IsType<SecretsManager>(secrets);
        }

        [Fact]
        public void AddSecretService_WithOptionsObject_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new SecretServiceOptions
            {
                DefaultProvider = "ParamStore",
                ParamStore = new ParamStoreOptions
                {
                    Region = "ap-southeast-1",
                    Credentials = new CredentialsOptions
                    {
                        AuthenticationType = "STS",
                        AccessKey = "test-key",
                        SecretKey = "test-secret",
                        SessionToken = "test-token"
                    }
                }
            };

            // Act
            services.AddSecretService(options);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var secretProvider = serviceProvider.GetService<ISecretServiceProvider>();
            Assert.NotNull(secretProvider);

            var secrets = serviceProvider.GetService<ISecrets>();
            Assert.NotNull(secrets);
        }

        [Fact]
        public void AddSecretService_WithConfiguration_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configDict = new Dictionary<string, string>
            {
                {"SecretService:DefaultProvider", "ParamStore"},
                {"SecretService:ParamStore:Region", "us-east-1"},
                {"SecretService:ParamStore:Credentials:AuthenticationType", "None"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            services.AddSecretService(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var secretProvider = serviceProvider.GetService<ISecretServiceProvider>();
            Assert.NotNull(secretProvider);

            var secrets = serviceProvider.GetService<ISecrets>();
            Assert.NotNull(secrets);
        }

        [Fact]
        public void CreateProvider_WithUnsupportedProvider_ThrowsNotSupportedException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSecretService(options =>
            {
                options.DefaultProvider = "UnsupportedProvider";
            });

            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                serviceProvider.GetService<ISecretServiceProvider>()
            );
        }

        [Fact]
        public void AddSecretService_WithNullServices_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ((IServiceCollection)null).AddSecretService(options => { })
            );
        }

        [Fact]
        public void AddSecretService_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services.AddSecretService((SecretServiceOptions)null)
            );
        }

        [Fact]
        public void AddSecretService_WithNullConfigureAction_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services.AddSecretService((Action<SecretServiceOptions>)null)
            );
        }

        [Fact]
        public void AddSecretService_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services.AddSecretService((IConfiguration)null)
            );
        }

        [Fact]
        public void AddSecretService_WithCustomSectionName_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configDict = new Dictionary<string, string>
            {
                {"CustomSecrets:DefaultProvider", "SecretsManager"},
                {"CustomSecrets:SecretsManager:Region", "us-west-2"},
                {"CustomSecrets:SecretsManager:Credentials:AuthenticationType", "AccessKey"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            services.AddSecretService(configuration, "CustomSecrets");
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var secretProvider = serviceProvider.GetService<ISecretServiceProvider>();
            Assert.NotNull(secretProvider);
            Assert.IsType<SecretsManagerProvider>(secretProvider);

            var secrets = serviceProvider.GetService<ISecrets>();
            Assert.NotNull(secrets);
            Assert.IsType<SecretsManager>(secrets);
        }
    }
}