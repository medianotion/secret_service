using Xunit;
using Security;
using Security.Providers;
using System;
using System.Reflection;
using System.Threading;

namespace unittest
{
    public class InterfaceTests
    {
        [Fact]
        public void ISecrets_Interface_HasCorrectMethods()
        {
            // Arrange
            var interfaceType = typeof(ISecrets);

            // Act
            var methods = interfaceType.GetMethods();

            // Assert
            Assert.Single(methods); // Should only have GetSecretAsync method
            
            var getSecretMethod = interfaceType.GetMethod("GetSecretAsync");
            Assert.NotNull(getSecretMethod);
            Assert.Equal(typeof(System.Threading.Tasks.Task<string>), getSecretMethod.ReturnType);
            
            var parameters = getSecretMethod.GetParameters();
            Assert.Equal(2, parameters.Length);
            Assert.Equal(typeof(string), parameters[0].ParameterType);
            Assert.Equal("secretKey", parameters[0].Name);
            Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
            Assert.Equal("cancellationToken", parameters[1].Name);
        }

        [Fact]
        public void ISecretServiceProvider_Interface_HasCorrectMethods()
        {
            // Arrange
            var interfaceType = typeof(ISecretServiceProvider);

            // Act
            var methods = interfaceType.GetMethods();

            // Assert
            Assert.Single(methods); // Should only have CreateService method
            
            var createServiceMethod = interfaceType.GetMethod("CreateService");
            Assert.NotNull(createServiceMethod);
            Assert.Equal(typeof(ISecrets), createServiceMethod.ReturnType);
            Assert.Empty(createServiceMethod.GetParameters()); // No parameters
        }

        [Fact]
        public void ParamStore_ImplementsISecrets()
        {
            // Act & Assert
            Assert.True(typeof(ISecrets).IsAssignableFrom(typeof(ParamStore)));
        }

        [Fact]
        public void SecretsManager_ImplementsISecrets()
        {
            // Act & Assert
            Assert.True(typeof(ISecrets).IsAssignableFrom(typeof(SecretsManager)));
        }

        [Fact]
        public void ParamStoreProvider_ImplementsISecretServiceProvider()
        {
            // Act & Assert
            Assert.True(typeof(ISecretServiceProvider).IsAssignableFrom(typeof(ParamStoreProvider)));
        }

        [Fact]
        public void SecretsManagerProvider_ImplementsISecretServiceProvider()
        {
            // Act & Assert
            Assert.True(typeof(ISecretServiceProvider).IsAssignableFrom(typeof(SecretsManagerProvider)));
        }

        [Fact]
        public void ISecrets_GetSecretAsync_HasCorrectSignature()
        {
            // Arrange
            var interfaceType = typeof(ISecrets);
            var method = interfaceType.GetMethod("GetSecretAsync");

            // Act & Assert
            Assert.NotNull(method);
            Assert.True(method.IsAbstract);
            Assert.False(method.IsStatic);
            Assert.True(method.IsPublic);
            
            // Check return type is Task<string>
            Assert.Equal(typeof(System.Threading.Tasks.Task<string>), method.ReturnType);
            
            // Check parameters
            var parameters = method.GetParameters();
            Assert.Equal(2, parameters.Length);
            
            // First parameter should be string secretKey
            Assert.Equal(typeof(string), parameters[0].ParameterType);
            Assert.Equal("secretKey", parameters[0].Name);
            
            // Second parameter should be CancellationToken cancellationToken
            Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
            Assert.Equal("cancellationToken", parameters[1].Name);
        }

        [Fact]
        public void ISecretServiceProvider_CreateService_HasCorrectSignature()
        {
            // Arrange
            var interfaceType = typeof(ISecretServiceProvider);
            var method = interfaceType.GetMethod("CreateService");

            // Act & Assert
            Assert.NotNull(method);
            Assert.True(method.IsAbstract);
            Assert.False(method.IsStatic);
            Assert.True(method.IsPublic);
            
            // Check return type is ISecrets
            Assert.Equal(typeof(ISecrets), method.ReturnType);
            
            // Check no parameters
            var parameters = method.GetParameters();
            Assert.Empty(parameters);
        }

        [Fact]
        public void ParamStore_HasPublicConstructor()
        {
            // Arrange
            var type = typeof(ParamStore);

            // Act
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // Assert
            Assert.NotEmpty(constructors);
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            Assert.Single(parameters);
            Assert.Equal(typeof(Security.Configuration.ParamStoreOptions), parameters[0].ParameterType);
        }

        [Fact]
        public void SecretsManager_HasPublicConstructor()
        {
            // Arrange
            var type = typeof(SecretsManager);

            // Act
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // Assert
            Assert.NotEmpty(constructors);
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            Assert.Single(parameters);
            Assert.Equal(typeof(Security.Configuration.SecretsManagerOptions), parameters[0].ParameterType);
        }

        [Fact]
        public void ParamStoreProvider_HasPublicConstructor()
        {
            // Arrange
            var type = typeof(ParamStoreProvider);

            // Act
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // Assert
            Assert.NotEmpty(constructors);
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            Assert.Single(parameters);
            Assert.Equal(typeof(Security.Configuration.ParamStoreOptions), parameters[0].ParameterType);
        }

        [Fact]
        public void SecretsManagerProvider_HasPublicConstructor()
        {
            // Arrange
            var type = typeof(SecretsManagerProvider);

            // Act
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // Assert
            Assert.NotEmpty(constructors);
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            Assert.Single(parameters);
            Assert.Equal(typeof(Security.Configuration.SecretsManagerOptions), parameters[0].ParameterType);
        }
    }
}