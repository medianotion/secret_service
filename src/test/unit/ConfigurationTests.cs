using Xunit;
using Security.Configuration;
using System;
using System.Collections.Generic;

namespace unittest
{
    public class ConfigurationTests
    {
        [Fact]
        public void SecretServiceOptions_DefaultValues_AreSet()
        {
            // Arrange & Act
            var options = new SecretServiceOptions();

            // Assert
            Assert.Equal("ParamStore", options.DefaultProvider);
            Assert.NotNull(options.Providers);
            Assert.Empty(options.Providers);
            Assert.NotNull(options.ParamStore);
            Assert.NotNull(options.SecretsManager);
        }

        [Fact]
        public void ParamStoreOptions_DefaultValues_AreSet()
        {
            // Arrange & Act
            var options = new ParamStoreOptions();

            // Assert
            Assert.Equal("us-east-1", options.Region);
            Assert.NotNull(options.Credentials);
            Assert.NotNull(options.Retry);
            Assert.NotNull(options.Properties);
            Assert.Equal(3, options.Retry.MaxRetries);
            Assert.Equal(2, options.Retry.DelaySeconds);
            Assert.True(options.Retry.Enabled);
        }

        [Fact]
        public void SecretsManagerOptions_DefaultValues_AreSet()
        {
            // Arrange & Act
            var options = new SecretsManagerOptions();

            // Assert
            Assert.Equal("us-east-1", options.Region);
            Assert.NotNull(options.Credentials);
            Assert.NotNull(options.Retry);
            Assert.NotNull(options.Properties);
            Assert.Equal(3, options.Retry.MaxRetries);
            Assert.Equal(2, options.Retry.DelaySeconds);
            Assert.True(options.Retry.Enabled);
        }

        [Fact]
        public void CredentialsOptions_DefaultValues_AreSet()
        {
            // Arrange & Act
            var credentials = new CredentialsOptions();

            // Assert
            Assert.Equal("None", credentials.AuthenticationType);
            Assert.Equal(string.Empty, credentials.AccessKey);
            Assert.Equal(string.Empty, credentials.SecretKey);
            Assert.Equal(string.Empty, credentials.SessionToken);
            Assert.NotNull(credentials.Properties);
            Assert.Empty(credentials.Properties);
        }

        [Fact]
        public void RetryOptions_DefaultValues_AreSet()
        {
            // Arrange & Act
            var retry = new RetryOptions();

            // Assert
            Assert.Equal(3, retry.MaxRetries);
            Assert.Equal(2, retry.DelaySeconds);
            Assert.True(retry.Enabled);
        }

        [Fact]
        public void AwsConnectionOptions_BackwardCompatibility_DelegatesToCredentials()
        {
            // Arrange
            var options = new AwsConnectionOptions();

            // Act
            options.AccessKey = "test-access-key";
            options.SecretKey = "test-secret-key";
            options.SessionToken = "test-session-token";

            // Assert
            Assert.Equal("test-access-key", options.Credentials.AccessKey);
            Assert.Equal("test-secret-key", options.Credentials.SecretKey);
            Assert.Equal("test-session-token", options.Credentials.SessionToken);
        }

        [Fact]
        public void AwsConnectionOptions_BackwardCompatibility_ReturnsCredentialValues()
        {
            // Arrange
            var options = new AwsConnectionOptions();
            options.Credentials.AccessKey = "test-access";
            options.Credentials.SecretKey = "test-secret";
            options.Credentials.SessionToken = "test-token";

            // Act & Assert
            Assert.Equal("test-access", options.AccessKey);
            Assert.Equal("test-secret", options.SecretKey);
            Assert.Equal("test-token", options.SessionToken);
        }

        [Fact]
        public void CredentialsOptions_SupportsDifferentAuthTypes()
        {
            // Test AccessKey authentication
            var accessKeyAuth = new CredentialsOptions
            {
                AuthenticationType = "AccessKey",
                AccessKey = "AKIA123",
                SecretKey = "secret123"
            };
            Assert.Equal("AccessKey", accessKeyAuth.AuthenticationType);
            Assert.Equal("AKIA123", accessKeyAuth.AccessKey);

            // Test STS authentication
            var stsAuth = new CredentialsOptions
            {
                AuthenticationType = "STS",
                AccessKey = "AKIA123",
                SecretKey = "secret123",
                SessionToken = "session123"
            };
            Assert.Equal("STS", stsAuth.AuthenticationType);
            Assert.Equal("session123", stsAuth.SessionToken);

            // Test Custom authentication
            var customAuth = new CredentialsOptions
            {
                AuthenticationType = "Custom",
                Properties = new Dictionary<string, string>
                {
                    { "endpoint", "https://vault.company.com" },
                    { "token", "hvs.123" }
                }
            };
            Assert.Equal("Custom", customAuth.AuthenticationType);
            Assert.Equal(2, customAuth.Properties.Count);
            Assert.Equal("https://vault.company.com", customAuth.Properties["endpoint"]);
        }

        [Fact]
        public void SecretServiceOptions_SectionName_IsCorrect()
        {
            // Act & Assert
            Assert.Equal("SecretService", SecretServiceOptions.SectionName);
        }

        [Fact]
        public void ParamStoreOptions_InheritsFromAwsConnectionOptions()
        {
            // Arrange & Act
            var options = new ParamStoreOptions();

            // Assert - Should have AwsConnectionOptions properties
            Assert.Equal("us-east-1", options.Region);
            Assert.NotNull(options.Credentials);
            Assert.NotNull(options.Retry);
            Assert.NotNull(options.Properties);
        }

        [Fact]
        public void SecretsManagerOptions_InheritsFromAwsConnectionOptions()
        {
            // Arrange & Act
            var options = new SecretsManagerOptions();

            // Assert - Should have AwsConnectionOptions properties
            Assert.Equal("us-east-1", options.Region);
            Assert.NotNull(options.Credentials);
            Assert.NotNull(options.Retry);
            Assert.NotNull(options.Properties);
        }

        [Fact]
        public void AwsConnectionOptions_DefaultValues_AreSet()
        {
            // Arrange & Act
            var options = new AwsConnectionOptions();

            // Assert
            Assert.Equal("us-east-1", options.Region);
            Assert.NotNull(options.Credentials);
            Assert.NotNull(options.Retry);
            Assert.NotNull(options.Properties);
            Assert.Empty(options.Properties);
        }

        [Fact]
        public void ParamStoreOptions_SupportsSTSCredentials()
        {
            // Arrange & Act
            var options = new ParamStoreOptions();
            options.Credentials.AccessKey = "AKIA...";
            options.Credentials.SecretKey = "secret...";
            options.Credentials.SessionToken = "session...";
            options.Credentials.AuthenticationType = "STS";

            // Assert
            Assert.Equal("AKIA...", options.AccessKey);
            Assert.Equal("secret...", options.SecretKey);
            Assert.Equal("session...", options.SessionToken);
            Assert.Equal("STS", options.Credentials.AuthenticationType);
        }

        [Fact]
        public void SecretsManagerOptions_SupportsSTSCredentials()
        {
            // Arrange & Act
            var options = new SecretsManagerOptions();
            options.Credentials.AccessKey = "AKIA...";
            options.Credentials.SecretKey = "secret...";
            options.Credentials.SessionToken = "session...";
            options.Credentials.AuthenticationType = "STS";

            // Assert
            Assert.Equal("AKIA...", options.AccessKey);
            Assert.Equal("secret...", options.SecretKey);
            Assert.Equal("session...", options.SessionToken);
            Assert.Equal("STS", options.Credentials.AuthenticationType);
        }

        [Fact]
        public void CredentialsOptions_SupportsCustomProperties()
        {
            // Arrange & Act
            var credentials = new CredentialsOptions
            {
                AuthenticationType = "Custom",
                Properties = new Dictionary<string, string>
                {
                    { "vault_endpoint", "https://vault.example.com" },
                    { "vault_token", "hvs.CAESIJK..." },
                    { "vault_namespace", "admin/dev" }
                }
            };

            // Assert
            Assert.Equal("Custom", credentials.AuthenticationType);
            Assert.Equal(3, credentials.Properties.Count);
            Assert.Equal("https://vault.example.com", credentials.Properties["vault_endpoint"]);
            Assert.Equal("hvs.CAESIJK...", credentials.Properties["vault_token"]);
            Assert.Equal("admin/dev", credentials.Properties["vault_namespace"]);
        }

        [Fact]
        public void RetryOptions_CanBeDisabled()
        {
            // Arrange & Act
            var retry = new RetryOptions
            {
                Enabled = false,
                MaxRetries = 0,
                DelaySeconds = 0
            };

            // Assert
            Assert.False(retry.Enabled);
            Assert.Equal(0, retry.MaxRetries);
            Assert.Equal(0, retry.DelaySeconds);
        }
    }
}