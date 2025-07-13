using Xunit;
using Moq;
using Security;
using Security.Configuration;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace unittest
{
    public class CancellationTokenTests
    {
        [Fact]
        public async Task ParamStore_GetSecretAsync_WithCancellationToken_PassesToAwsClient()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var response = new GetParameterResponse
            {
                Parameter = new Parameter { Value = "test-value" }
            };

            var mock = new Mock<IAmazonSimpleSystemsManagement>();
            mock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), cancellationToken))
                .Returns(Task.FromResult(response))
                .Verifiable();

            var options = new ParamStoreOptions();
            var paramStore = new ParamStore(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(ParamStore).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(paramStore, mock.Object);

            // Act
            await paramStore.GetSecretAsync("test-key", cancellationToken);

            // Assert
            mock.Verify(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SecretsManager_GetSecretAsync_WithCancellationToken_PassesToAwsClient()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var response = new GetSecretValueResponse
            {
                SecretString = "test-value"
            };

            var mock = new Mock<IAmazonSecretsManager>();
            mock.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), cancellationToken))
                .Returns(Task.FromResult(response))
                .Verifiable();

            var options = new SecretsManagerOptions();
            var secretsManager = new SecretsManager(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(SecretsManager).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(secretsManager, mock.Object);

            // Act
            await secretsManager.GetSecretAsync("test-secret", cancellationToken);

            // Assert
            mock.Verify(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ParamStore_GetSecretAsync_WithDefaultCancellationToken_Works()
        {
            // Arrange
            var response = new GetParameterResponse
            {
                Parameter = new Parameter { Value = "test-value" }
            };

            var mock = new Mock<IAmazonSimpleSystemsManagement>();
            mock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));

            var options = new ParamStoreOptions();
            var paramStore = new ParamStore(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(ParamStore).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(paramStore, mock.Object);

            // Act & Assert - Should work without explicit CancellationToken
            var result = await paramStore.GetSecretAsync("test-key");
            Assert.Equal("test-value", result);

            // Verify method was called with some CancellationToken (could be default)
            mock.Verify(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SecretsManager_GetSecretAsync_WithDefaultCancellationToken_Works()
        {
            // Arrange
            var response = new GetSecretValueResponse
            {
                SecretString = "test-value"
            };

            var mock = new Mock<IAmazonSecretsManager>();
            mock.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));

            var options = new SecretsManagerOptions();
            var secretsManager = new SecretsManager(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(SecretsManager).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(secretsManager, mock.Object);

            // Act & Assert - Should work without explicit CancellationToken
            var result = await secretsManager.GetSecretAsync("test-secret");
            Assert.Equal("test-value", result);

            // Verify method was called with some CancellationToken (could be default)
            mock.Verify(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void ISecrets_Interface_GetSecretAsync_HasCancellationTokenParameter()
        {
            // Arrange
            var interfaceType = typeof(ISecrets);

            // Act
            var method = interfaceType.GetMethod("GetSecretAsync");

            // Assert
            Assert.NotNull(method);
            var parameters = method.GetParameters();
            Assert.Equal(2, parameters.Length);
            Assert.Equal(typeof(string), parameters[0].ParameterType);
            Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
        }

        [Fact]
        public async Task ParamStore_GetSecretAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var cancellationToken = cancellationTokenSource.Token;

            var mock = new Mock<IAmazonSimpleSystemsManagement>();
            mock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), cancellationToken))
                .ThrowsAsync(new OperationCanceledException());

            var options = new ParamStoreOptions();
            var paramStore = new ParamStore(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(ParamStore).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(paramStore, mock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await paramStore.GetSecretAsync("test-key", cancellationToken));
        }

        [Fact]
        public async Task SecretsManager_GetSecretAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var cancellationToken = cancellationTokenSource.Token;

            var mock = new Mock<IAmazonSecretsManager>();
            mock.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), cancellationToken))
                .ThrowsAsync(new OperationCanceledException());

            var options = new SecretsManagerOptions();
            var secretsManager = new SecretsManager(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(SecretsManager).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(secretsManager, mock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await secretsManager.GetSecretAsync("test-secret", cancellationToken));
        }

        [Fact]
        public async Task ParamStore_GetSecretAsync_PassesCorrectRequestParameters()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var secretKey = "test-parameter-key";
            var response = new GetParameterResponse
            {
                Parameter = new Parameter { Value = "test-value" }
            };

            var mock = new Mock<IAmazonSimpleSystemsManagement>();
            mock.Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), cancellationToken))
                .Returns(Task.FromResult(response))
                .Verifiable();

            var options = new ParamStoreOptions();
            var paramStore = new ParamStore(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(ParamStore).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(ParamStore).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(paramStore, mock.Object);

            // Act
            await paramStore.GetSecretAsync(secretKey, cancellationToken);

            // Assert
            mock.Verify(x => x.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == secretKey && r.WithDecryption == true), 
                cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SecretsManager_GetSecretAsync_PassesCorrectRequestParameters()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var secretKey = "test-secret-key";
            var response = new GetSecretValueResponse
            {
                SecretString = "test-value"
            };

            var mock = new Mock<IAmazonSecretsManager>();
            mock.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), cancellationToken))
                .Returns(Task.FromResult(response))
                .Verifiable();

            var options = new SecretsManagerOptions();
            var secretsManager = new SecretsManager(options);

            // Clear cache to ensure test isolation
            var cacheField = typeof(SecretsManager).GetField("Cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cache = (Dictionary<string, string>)cacheField.GetValue(null);
            cache.Clear();

            // Use reflection to set the mock client for testing
            var clientField = typeof(SecretsManager).GetField("_amazonClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(secretsManager, mock.Object);

            // Act
            await secretsManager.GetSecretAsync(secretKey, cancellationToken);

            // Assert
            mock.Verify(x => x.GetSecretValueAsync(
                It.Is<GetSecretValueRequest>(r => r.SecretId == secretKey), 
                cancellationToken), Times.Once);
        }
    }
}