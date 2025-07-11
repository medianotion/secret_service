using Xunit;
using Moq;
using System;
using Security;
using Security.Configuration;
using Security.Providers;
using Security.Extensions;
using Security.Exceptions;
using Amazon.SimpleSystemsManagement;
using Amazon.SecretsManager;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement.Model;
using System.Threading;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace unittest
{
    public class UnitTest1
    {
        [Fact]
        public async Task ParamStoreGetSecret()
        {
            string expected = "value";
            var mock = new Mock<IAmazonSimpleSystemsManagement>();

            var response = new GetParameterResponse()
            {
                Parameter = new Parameter() { Value = expected },
            };

            mock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), default(CancellationToken)))
                .Returns(Task.FromResult(response));

            var options = new ParamStoreOptions();
            var ss = new ParamStore(options);
            
            // Use reflection to set the mock client for testing
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(ss, mock.Object);

            Assert.Equal(expected, await ss.GetSecretAsync("key"));
        }

        [Fact]
        public async Task ParamStoreUseLocalCache()
        {
            var mock = new Mock<IAmazonSimpleSystemsManagement>();
            mock.Setup(m => m.GetParameterAsync(
                It.IsAny<GetParameterRequest>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new GetParameterResponse { Parameter = new Parameter { Value = "b" } });

            var options = new ParamStoreOptions();
            var sm = new ParamStore(options);
            
            // Use reflection to set the mock client for testing
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(sm, mock.Object);

            await sm.GetSecretAsync("a");
            await sm.GetSecretAsync("a");

            mock.Verify(x =>
                    x.GetParameterAsync(
                        It.Is<GetParameterRequest>(r => r.Name == "a" && r.WithDecryption == true),
                        It.IsAny<CancellationToken>()),
                Times.Once
            );

            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ParamStoreRefreshLocalCache()
        {
            var mock = new Mock<IAmazonSimpleSystemsManagement>();
            mock.Setup(m => m.GetParameterAsync(
                It.IsAny<GetParameterRequest>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new GetParameterResponse { Parameter = new Parameter { Value = "b" } });

            var options = new ParamStoreOptions();
            var sm = new ParamStore(options);
            
            // Use reflection to set the mock client for testing
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(sm, mock.Object);

            ParamStore.SetCacheRefreshTimeTo(1);
            await sm.GetSecretAsync("a");
            await sm.GetSecretAsync("a");

            await Task.Delay(1200);
            await sm.GetSecretAsync("a");
            await sm.GetSecretAsync("a");

            mock.Verify(x =>
                    x.GetParameterAsync(
                        It.Is<GetParameterRequest>(r => r.Name == "a" && r.WithDecryption == true),
                        It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );

            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ParamStoreGetSecretNullKey()
        {
            var options = new ParamStoreOptions();
            var ss = new ParamStore(options);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ss.GetSecretAsync(null));
        }

        [Fact]
        public async Task ParamStoreGetSecretEmptyKey()
        {
            var options = new ParamStoreOptions();
            var ss = new ParamStore(options);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ss.GetSecretAsync(""));
        }

        [Fact]
        public async Task SecretsManagerGetSecret()
        {
            string expected = "value";
            var mock = new Mock<IAmazonSecretsManager>();

            var response = new GetSecretValueResponse()
            {
                SecretString = expected
            };

            mock.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), default(CancellationToken)))
                .Returns(Task.FromResult(response));

            var options = new SecretsManagerOptions();
            var ss = new SecretsManager(options);
            
            // Use reflection to set the mock client for testing
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(ss, mock.Object);

            Assert.Equal(expected, await ss.GetSecretAsync("key"));
        }

        [Fact]
        public async Task SecretsManagerUseLocalCache()
        {
            var mock = new Mock<IAmazonSecretsManager>();
            mock.Setup(m => m.GetSecretValueAsync(
                It.IsAny<GetSecretValueRequest>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new GetSecretValueResponse { SecretString = "b" });

            var options = new SecretsManagerOptions();
            var sm = new SecretsManager(options);
            
            // Use reflection to set the mock client for testing
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(sm, mock.Object);

            await sm.GetSecretAsync("a");
            await sm.GetSecretAsync("a");

            mock.Verify(x =>
                    x.GetSecretValueAsync(
                        It.Is<GetSecretValueRequest>(r => r.SecretId == "a"),
                        It.IsAny<CancellationToken>()),
                Times.Once
            );

            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SecretsManagerRefreshLocalCache()
        {
            var mock = new Mock<IAmazonSecretsManager>();
            mock.Setup(m => m.GetSecretValueAsync(
                It.IsAny<GetSecretValueRequest>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new GetSecretValueResponse { SecretString = "b" });

            var options = new SecretsManagerOptions();
            var sm = new SecretsManager(options);
            
            // Use reflection to set the mock client for testing
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(sm, mock.Object);

            SecretsManager.SetCacheRefreshTimeTo(1);
            await sm.GetSecretAsync("a");
            await sm.GetSecretAsync("a");

            await Task.Delay(1200);
            await sm.GetSecretAsync("a");
            await sm.GetSecretAsync("a");

            mock.Verify(x =>
                    x.GetSecretValueAsync(
                        It.Is<GetSecretValueRequest>(r => r.SecretId == "a"),
                        It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );

            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SecretsManagerSecretNullKey()
        {
            var options = new SecretsManagerOptions();
            var ss = new SecretsManager(options);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ss.GetSecretAsync(null));
        }

        [Fact]
        public async Task SecretsManagerGetSecretEmptyKey()
        {
            var options = new SecretsManagerOptions();
            var ss = new SecretsManager(options);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ss.GetSecretAsync(""));
        }

        [Fact]
        public void ParamStoreProviderCreateService()
        {
            var options = new ParamStoreOptions();
            var provider = new ParamStoreProvider(options);
            Assert.IsType<ParamStore>(provider.CreateService());
        }

        [Fact]
        public void SecretsManagerProviderCreateService()
        {
            var options = new SecretsManagerOptions();
            var provider = new SecretsManagerProvider(options);
            Assert.IsType<SecretsManager>(provider.CreateService());
        }

        [Fact]
        public void ParamStoreOptionsDefaults()
        {
            var options = new ParamStoreOptions();
            Assert.Equal("us-east-1", options.Region);
            Assert.Equal(3, options.Retry.MaxRetries);
            Assert.Equal(2, options.Retry.DelaySeconds);
            Assert.True(options.Retry.Enabled);
        }

        [Fact]
        public void SecretsManagerOptionsDefaults()
        {
            var options = new SecretsManagerOptions();
            Assert.Equal("us-east-1", options.Region);
            Assert.Equal(3, options.Retry.MaxRetries);
            Assert.Equal(2, options.Retry.DelaySeconds);
            Assert.True(options.Retry.Enabled);
        }

        [Fact]
        public void SecretServiceOptionsDefaults()
        {
            var options = new SecretServiceOptions();
            Assert.Equal("ParamStore", options.DefaultProvider);
            Assert.NotNull(options.ParamStore);
            Assert.NotNull(options.SecretsManager);
            Assert.NotNull(options.Providers);
        }

        [Fact]
        public void ServiceCollectionExtensionsAddSecretServiceWithOptions()
        {
            var services = new ServiceCollection();
            var options = new SecretServiceOptions
            {
                DefaultProvider = "ParamStore",
                ParamStore = new ParamStoreOptions { Region = "us-west-2" }
            };

            services.AddSecretService(options);

            var serviceProvider = services.BuildServiceProvider();
            var secretService = serviceProvider.GetService<ISecrets>();
            
            Assert.NotNull(secretService);
        }

        [Fact]
        public void ServiceCollectionExtensionsAddSecretServiceWithAction()
        {
            var services = new ServiceCollection();

            services.AddSecretService(options =>
            {
                options.DefaultProvider = "SecretsManager";
                options.SecretsManager.Region = "eu-west-1";
            });

            var serviceProvider = services.BuildServiceProvider();
            var secretService = serviceProvider.GetService<ISecrets>();
            
            Assert.NotNull(secretService);
        }

        [Fact]
        public void ValidateSecretsKeyEmptySecretKey()
        {
            Assert.Throws<ArgumentNullException>(() => Security.Helpers.ValidateSecretKey(""));
        }

        [Fact]
        public void ValidateSecretsKeyNullSecretKey()
        {
            Assert.Throws<ArgumentNullException>(() => Security.Helpers.ValidateSecretKey(null));
        }

        [Fact]
        public void ParamStoreOptionsSupportsSTSCredentials()
        {
            var options = new ParamStoreOptions();
            options.Credentials.AccessKey = "AKIA...";
            options.Credentials.SecretKey = "secret...";
            options.Credentials.SessionToken = "session...";
            options.Credentials.AuthenticationType = "STS";

            Assert.Equal("AKIA...", options.AccessKey);
            Assert.Equal("secret...", options.SecretKey);
            Assert.Equal("session...", options.SessionToken);
            Assert.Equal("STS", options.Credentials.AuthenticationType);
        }

        [Fact]
        public void SecretsManagerOptionsSupportsSTSCredentials()
        {
            var options = new SecretsManagerOptions();
            options.Credentials.AccessKey = "AKIA...";
            options.Credentials.SecretKey = "secret...";
            options.Credentials.SessionToken = "session...";
            options.Credentials.AuthenticationType = "STS";

            Assert.Equal("AKIA...", options.AccessKey);
            Assert.Equal("secret...", options.SecretKey);
            Assert.Equal("session...", options.SessionToken);
            Assert.Equal("STS", options.Credentials.AuthenticationType);
        }

        [Fact]
        public void CredentialsOptionsDefaults()
        {
            var credentials = new CredentialsOptions();
            Assert.Equal("None", credentials.AuthenticationType);
            Assert.Equal(string.Empty, credentials.AccessKey);
            Assert.Equal(string.Empty, credentials.SecretKey);
            Assert.Equal(string.Empty, credentials.SessionToken);
            Assert.NotNull(credentials.Properties);
        }

        [Fact]
        public void AwsConnectionOptionsDefaults()
        {
            var options = new AwsConnectionOptions();
            Assert.Equal("us-east-1", options.Region);
            Assert.NotNull(options.Credentials);
            Assert.NotNull(options.Retry);
            Assert.NotNull(options.Properties);
        }

        [Fact]
        public void ParamStoreWithSTSCredentials()
        {
            var options = new ParamStoreOptions();
            options.AccessKey = "AKIA...";
            options.SecretKey = "secret...";
            options.SessionToken = "session...";

            // Should not throw - backward compatibility maintained
            Assert.NotNull(options);
            Assert.Equal("AKIA...", options.Credentials.AccessKey);
            Assert.Equal("secret...", options.Credentials.SecretKey);
            Assert.Equal("session...", options.Credentials.SessionToken);
        }

        [Fact]
        public void SecretsManagerWithSTSCredentials()
        {
            var options = new SecretsManagerOptions();
            options.AccessKey = "AKIA...";
            options.SecretKey = "secret...";
            options.SessionToken = "session...";

            // Should not throw - backward compatibility maintained
            Assert.NotNull(options);
            Assert.Equal("AKIA...", options.Credentials.AccessKey);
            Assert.Equal("secret...", options.Credentials.SecretKey);
            Assert.Equal("session...", options.Credentials.SessionToken);
        }

        [Fact]
        public async Task ParamStoreWrapsParameterNotFoundException()
        {
            var mock = new Mock<IAmazonSimpleSystemsManagement>();
            mock.Setup(m => m.GetParameterAsync(
                It.IsAny<GetParameterRequest>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new ParameterNotFoundException("Parameter not found"));

            var options = new ParamStoreOptions();
            var paramStore = new ParamStore(options);
            
            // Use reflection to set the mock client
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(paramStore, mock.Object);

            var exception = await Assert.ThrowsAsync<SecretNotFoundException>(
                async () => await paramStore.GetSecretAsync("nonexistent-key"));
            
            Assert.Equal("nonexistent-key", exception.SecretKey);
            Assert.IsType<ParameterNotFoundException>(exception.InnerException);
        }

        [Fact]
        public async Task SecretsManagerWrapsResourceNotFoundException()
        {
            var mock = new Mock<IAmazonSecretsManager>();
            mock.Setup(m => m.GetSecretValueAsync(
                It.IsAny<GetSecretValueRequest>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Amazon.SecretsManager.Model.ResourceNotFoundException("Secret not found"));

            var options = new SecretsManagerOptions();
            var secretsManager = new SecretsManager(options);
            
            // Use reflection to set the mock client
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(secretsManager, mock.Object);

            var exception = await Assert.ThrowsAsync<SecretNotFoundException>(
                async () => await secretsManager.GetSecretAsync("nonexistent-secret"));
            
            Assert.Equal("nonexistent-secret", exception.SecretKey);
            Assert.IsType<Amazon.SecretsManager.Model.ResourceNotFoundException>(exception.InnerException);
        }

        [Fact]
        public void SecretNotFoundExceptionProperties()
        {
            var exception = new SecretNotFoundException("test-key");
            Assert.Equal("test-key", exception.SecretKey);
            Assert.Contains("test-key", exception.Message);
        }

        [Fact]
        public void SecretAccessDeniedExceptionProperties()
        {
            var exception = new SecretAccessDeniedException("test-key");
            Assert.Equal("test-key", exception.SecretKey);
            Assert.Contains("test-key", exception.Message);
            Assert.Contains("Access denied", exception.Message);
        }

        [Fact]
        public void SecretServiceExceptionHierarchy()
        {
            var notFoundEx = new SecretNotFoundException("key");
            var accessDeniedEx = new SecretAccessDeniedException("key");
            var authEx = new SecretAuthenticationException("auth failed");
            var configEx = new SecretConfigurationException("config invalid");
            var unavailableEx = new SecretServiceUnavailableException("service down");
            var timeoutEx = new SecretTimeoutException("timeout");
            var internalEx = new SecretServiceInternalException("internal error");

            // All should inherit from SecretServiceException
            Assert.IsAssignableFrom<SecretServiceException>(notFoundEx);
            Assert.IsAssignableFrom<SecretServiceException>(accessDeniedEx);
            Assert.IsAssignableFrom<SecretServiceException>(authEx);
            Assert.IsAssignableFrom<SecretServiceException>(configEx);
            Assert.IsAssignableFrom<SecretServiceException>(unavailableEx);
            Assert.IsAssignableFrom<SecretServiceException>(timeoutEx);
            Assert.IsAssignableFrom<SecretServiceException>(internalEx);
        }
    }
}