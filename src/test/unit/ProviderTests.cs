using Xunit;
using Security.Providers;
using Security.Configuration;
using Security;
using System;

namespace unittest
{
    public class ProviderTests
    {
        [Fact]
        public void ParamStoreProvider_Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ParamStoreProvider(null));
        }

        [Fact]
        public void ParamStoreProvider_Constructor_WithValidOptions_CreatesProvider()
        {
            // Arrange
            var options = new ParamStoreOptions
            {
                Region = "us-west-2",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "None"
                }
            };

            // Act
            var provider = new ParamStoreProvider(options);

            // Assert
            Assert.NotNull(provider);
            Assert.IsAssignableFrom<ISecretServiceProvider>(provider);
        }

        [Fact]
        public void ParamStoreProvider_CreateService_ReturnsValidService()
        {
            // Arrange
            var options = new ParamStoreOptions
            {
                Region = "eu-west-1",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "AccessKey",
                    AccessKey = "AKIA123",
                    SecretKey = "secret123"
                }
            };
            var provider = new ParamStoreProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ISecrets>(service);
            Assert.IsType<ParamStore>(service);
        }

        [Fact]
        public void ParamStoreProvider_CreateService_WithSTSCredentials_ReturnsValidService()
        {
            // Arrange
            var options = new ParamStoreOptions
            {
                Region = "ap-southeast-1",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "STS",
                    AccessKey = "AKIA123",
                    SecretKey = "secret123",
                    SessionToken = "session123"
                }
            };
            var provider = new ParamStoreProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<ParamStore>(service);
        }

        [Fact]
        public void ParamStoreProvider_CreateService_WithDefaultCredentials_ReturnsValidService()
        {
            // Arrange
            var options = new ParamStoreOptions
            {
                Region = "us-east-1",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "None" // Use default AWS credentials
                }
            };
            var provider = new ParamStoreProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<ParamStore>(service);
        }

        [Fact]
        public void SecretsManagerProvider_Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SecretsManagerProvider(null));
        }

        [Fact]
        public void SecretsManagerProvider_Constructor_WithValidOptions_CreatesProvider()
        {
            // Arrange
            var options = new SecretsManagerOptions
            {
                Region = "us-west-2",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "None"
                }
            };

            // Act
            var provider = new SecretsManagerProvider(options);

            // Assert
            Assert.NotNull(provider);
            Assert.IsAssignableFrom<ISecretServiceProvider>(provider);
        }

        [Fact]
        public void SecretsManagerProvider_CreateService_ReturnsValidService()
        {
            // Arrange
            var options = new SecretsManagerOptions
            {
                Region = "eu-west-1",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "AccessKey",
                    AccessKey = "AKIA123",
                    SecretKey = "secret123"
                }
            };
            var provider = new SecretsManagerProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ISecrets>(service);
            Assert.IsType<SecretsManager>(service);
        }

        [Fact]
        public void SecretsManagerProvider_CreateService_WithSTSCredentials_ReturnsValidService()
        {
            // Arrange
            var options = new SecretsManagerOptions
            {
                Region = "ap-southeast-1",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "STS",
                    AccessKey = "AKIA123",
                    SecretKey = "secret123",
                    SessionToken = "session123"
                }
            };
            var provider = new SecretsManagerProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<SecretsManager>(service);
        }

        [Fact]
        public void SecretsManagerProvider_CreateService_WithDefaultCredentials_ReturnsValidService()
        {
            // Arrange
            var options = new SecretsManagerOptions
            {
                Region = "us-east-1",
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "None" // Use default AWS credentials
                }
            };
            var provider = new SecretsManagerProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<SecretsManager>(service);
        }

        [Fact]
        public void ISecretServiceProvider_Interface_HasCorrectSignature()
        {
            // Arrange
            var interfaceType = typeof(ISecretServiceProvider);

            // Act
            var createServiceMethod = interfaceType.GetMethod("CreateService");

            // Assert
            Assert.NotNull(createServiceMethod);
            Assert.Equal(typeof(ISecrets), createServiceMethod.ReturnType);
            Assert.Empty(createServiceMethod.GetParameters());
        }

        [Fact]
        public void ParamStoreProvider_CreateService_WithCustomRegion_ReturnsValidService()
        {
            // Arrange
            var options = new ParamStoreOptions
            {
                Region = "sa-east-1", // Custom region
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "AccessKey",
                    AccessKey = "test",
                    SecretKey = "test"
                }
            };
            var provider = new ParamStoreProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<ParamStore>(service);
        }

        [Fact]
        public void SecretsManagerProvider_CreateService_WithCustomRegion_ReturnsValidService()
        {
            // Arrange
            var options = new SecretsManagerOptions
            {
                Region = "ca-central-1", // Custom region
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "AccessKey",
                    AccessKey = "test",
                    SecretKey = "test"
                }
            };
            var provider = new SecretsManagerProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<SecretsManager>(service);
        }

        [Fact]
        public void ParamStoreProvider_CreateService_WithRetryConfiguration_ReturnsValidService()
        {
            // Arrange
            var options = new ParamStoreOptions
            {
                Region = "us-east-1",
                Retry = new RetryOptions
                {
                    MaxRetries = 5,
                    DelaySeconds = 3,
                    Enabled = true
                },
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "None"
                }
            };
            var provider = new ParamStoreProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<ParamStore>(service);
        }

        [Fact]
        public void SecretsManagerProvider_CreateService_WithRetryConfiguration_ReturnsValidService()
        {
            // Arrange
            var options = new SecretsManagerOptions
            {
                Region = "us-east-1",
                Retry = new RetryOptions
                {
                    MaxRetries = 5,
                    DelaySeconds = 3,
                    Enabled = true
                },
                Credentials = new CredentialsOptions
                {
                    AuthenticationType = "None"
                }
            };
            var provider = new SecretsManagerProvider(options);

            // Act
            var service = provider.CreateService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<SecretsManager>(service);
        }
    }
}