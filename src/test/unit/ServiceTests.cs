using Xunit;
using Moq;
using System;
using Security;
using Security.Configuration;
using Security.Exceptions;
using Amazon.SimpleSystemsManagement;
using Amazon.SecretsManager;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement.Model;
using System.Threading;
using Amazon.SecretsManager.Model;

namespace unittest
{
    public class ServiceTests
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
        public void ValidateSecretsKeyEmptySecretKey()
        {
            Assert.Throws<ArgumentNullException>(() => Security.Helpers.ValidateSecretKey(""));
        }

        [Fact]
        public void ValidateSecretsKeyNullSecretKey()
        {
            Assert.Throws<ArgumentNullException>(() => Security.Helpers.ValidateSecretKey(null));
        }
    }
}