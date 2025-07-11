using System;

namespace Security.Exceptions
{
    /// <summary>
    /// Base exception for all secret service operations
    /// </summary>
    public abstract class SecretServiceException : Exception
    {
        protected SecretServiceException(string message) : base(message) { }
        protected SecretServiceException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a secret is not found
    /// </summary>
    public class SecretNotFoundException : SecretServiceException
    {
        public string SecretKey { get; }

        public SecretNotFoundException(string secretKey) 
            : base($"Secret '{secretKey}' was not found")
        {
            SecretKey = secretKey;
        }

        public SecretNotFoundException(string secretKey, Exception innerException) 
            : base($"Secret '{secretKey}' was not found", innerException)
        {
            SecretKey = secretKey;
        }
    }

    /// <summary>
    /// Exception thrown when secret access is denied due to insufficient permissions
    /// </summary>
    public class SecretAccessDeniedException : SecretServiceException
    {
        public string SecretKey { get; }

        public SecretAccessDeniedException(string secretKey) 
            : base($"Access denied to secret '{secretKey}'. Check IAM permissions.")
        {
            SecretKey = secretKey;
        }

        public SecretAccessDeniedException(string secretKey, Exception innerException) 
            : base($"Access denied to secret '{secretKey}'. Check IAM permissions.", innerException)
        {
            SecretKey = secretKey;
        }
    }

    /// <summary>
    /// Exception thrown when there are authentication issues with the secret service
    /// </summary>
    public class SecretAuthenticationException : SecretServiceException
    {
        public SecretAuthenticationException(string message) : base(message) { }
        public SecretAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when secret service configuration is invalid
    /// </summary>
    public class SecretConfigurationException : SecretServiceException
    {
        public SecretConfigurationException(string message) : base(message) { }
        public SecretConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when secret service is temporarily unavailable or rate limited
    /// </summary>
    public class SecretServiceUnavailableException : SecretServiceException
    {
        public SecretServiceUnavailableException(string message) : base(message) { }
        public SecretServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a secret operation times out
    /// </summary>
    public class SecretTimeoutException : SecretServiceException
    {
        public SecretTimeoutException(string message) : base(message) { }
        public SecretTimeoutException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown for unexpected secret service errors
    /// </summary>
    public class SecretServiceInternalException : SecretServiceException
    {
        public SecretServiceInternalException(string message) : base(message) { }
        public SecretServiceInternalException(string message, Exception innerException) : base(message, innerException) { }
    }
}