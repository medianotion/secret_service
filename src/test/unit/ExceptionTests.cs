using Xunit;
using Security.Exceptions;
using System;

namespace unittest
{
    public class ExceptionTests
    {
        [Fact]
        public void SecretNotFoundException_Constructor_WithSecretKey_SetsProperties()
        {
            // Arrange
            var secretKey = "my-secret-key";

            // Act
            var exception = new SecretNotFoundException(secretKey);

            // Assert
            Assert.Equal(secretKey, exception.SecretKey);
            Assert.Contains(secretKey, exception.Message);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public void SecretNotFoundException_Constructor_WithInnerException_PreservesInnerException()
        {
            // Arrange
            var secretKey = "test-secret";
            var innerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new SecretNotFoundException(secretKey, innerException);

            // Assert
            Assert.Equal(secretKey, exception.SecretKey);
            Assert.Equal(innerException, exception.InnerException);
            Assert.Contains(secretKey, exception.Message);
        }

        [Fact]
        public void SecretAccessDeniedException_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var secretKey = "test-secret";

            // Act
            var exception = new SecretAccessDeniedException(secretKey);

            // Assert
            Assert.Equal(secretKey, exception.SecretKey);
            Assert.Contains(secretKey, exception.Message);
            Assert.Contains("Access denied", exception.Message);
            Assert.Contains("Check IAM permissions", exception.Message);
        }

        [Fact]
        public void SecretAuthenticationException_Constructor_SetsMessage()
        {
            // Arrange
            var message = "Authentication failed";

            // Act
            var exception = new SecretAuthenticationException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void SecretConfigurationException_Constructor_SetsMessage()
        {
            // Arrange
            var message = "Configuration error";

            // Act
            var exception = new SecretConfigurationException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void SecretServiceUnavailableException_Constructor_SetsMessage()
        {
            // Arrange
            var message = "Service temporarily unavailable";

            // Act
            var exception = new SecretServiceUnavailableException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void SecretTimeoutException_Constructor_SetsMessage()
        {
            // Arrange
            var message = "Operation timed out";

            // Act
            var exception = new SecretTimeoutException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void SecretServiceInternalException_Constructor_SetsMessage()
        {
            // Arrange
            var message = "Internal service error";

            // Act
            var exception = new SecretServiceInternalException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void SecretServiceException_IsAbstract()
        {
            // Act & Assert
            Assert.True(typeof(SecretServiceException).IsAbstract);
        }

        [Fact]
        public void AllCustomExceptions_InheritFromSecretServiceException()
        {
            // Act & Assert
            Assert.True(typeof(SecretNotFoundException).IsSubclassOf(typeof(SecretServiceException)));
            Assert.True(typeof(SecretAccessDeniedException).IsSubclassOf(typeof(SecretServiceException)));
            Assert.True(typeof(SecretAuthenticationException).IsSubclassOf(typeof(SecretServiceException)));
            Assert.True(typeof(SecretConfigurationException).IsSubclassOf(typeof(SecretServiceException)));
            Assert.True(typeof(SecretServiceUnavailableException).IsSubclassOf(typeof(SecretServiceException)));
            Assert.True(typeof(SecretTimeoutException).IsSubclassOf(typeof(SecretServiceException)));
            Assert.True(typeof(SecretServiceInternalException).IsSubclassOf(typeof(SecretServiceException)));
        }

        [Fact]
        public void SecretServiceException_InheritsFromException()
        {
            // Act & Assert
            Assert.True(typeof(SecretServiceException).IsSubclassOf(typeof(Exception)));
        }

        [Fact]
        public void SecretExceptions_WithInnerException_PreserveInnerException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Original error");

            // Act
            var secretNotFound = new SecretNotFoundException("secret", innerException);
            var accessDenied = new SecretAccessDeniedException("secret", innerException);
            var authException = new SecretAuthenticationException("auth error", innerException);
            var configException = new SecretConfigurationException("config error", innerException);
            var unavailableException = new SecretServiceUnavailableException("unavailable", innerException);
            var timeoutException = new SecretTimeoutException("timeout", innerException);
            var internalException = new SecretServiceInternalException("internal", innerException);

            // Assert
            Assert.Equal(innerException, secretNotFound.InnerException);
            Assert.Equal(innerException, accessDenied.InnerException);
            Assert.Equal(innerException, authException.InnerException);
            Assert.Equal(innerException, configException.InnerException);
            Assert.Equal(innerException, unavailableException.InnerException);
            Assert.Equal(innerException, timeoutException.InnerException);
            Assert.Equal(innerException, internalException.InnerException);
        }

        [Fact]
        public void SecretNotFoundException_WithoutInnerException_HasNullInnerException()
        {
            // Arrange & Act
            var exception = new SecretNotFoundException("test-secret");

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SecretAccessDeniedException_WithoutInnerException_HasNullInnerException()
        {
            // Arrange & Act
            var exception = new SecretAccessDeniedException("test-secret");

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SecretAuthenticationException_WithoutInnerException_HasNullInnerException()
        {
            // Arrange & Act
            var exception = new SecretAuthenticationException("Authentication failed");

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SecretConfigurationException_WithoutInnerException_HasNullInnerException()
        {
            // Arrange & Act
            var exception = new SecretConfigurationException("Configuration error");

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SecretServiceUnavailableException_WithoutInnerException_HasNullInnerException()
        {
            // Arrange & Act
            var exception = new SecretServiceUnavailableException("Service unavailable");

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SecretTimeoutException_WithoutInnerException_HasNullInnerException()
        {
            // Arrange & Act
            var exception = new SecretTimeoutException("Timeout occurred");

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SecretServiceInternalException_WithoutInnerException_HasNullInnerException()
        {
            // Arrange & Act
            var exception = new SecretServiceInternalException("Internal error");

            // Assert
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SecretNotFoundException_MessageFormat_IsCorrect()
        {
            // Arrange
            var secretKey = "test-key-123";

            // Act
            var exception = new SecretNotFoundException(secretKey);

            // Assert
            Assert.Equal($"Secret '{secretKey}' was not found", exception.Message);
        }

        [Fact]
        public void SecretAccessDeniedException_MessageFormat_IsCorrect()
        {
            // Arrange
            var secretKey = "test-key-123";

            // Act
            var exception = new SecretAccessDeniedException(secretKey);

            // Assert
            Assert.Equal($"Access denied to secret '{secretKey}'. Check IAM permissions.", exception.Message);
        }
    }
}