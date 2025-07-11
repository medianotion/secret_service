using System.Collections.Generic;

namespace Security.Configuration
{
    /// <summary>
    /// Generic credentials configuration that can support various authentication mechanisms
    /// </summary>
    public class CredentialsOptions
    {
        /// <summary>
        /// Authentication type: "None" (IAM role), "AccessKey", "STS", "Custom"
        /// </summary>
        public string AuthenticationType { get; set; } = "None";
        
        /// <summary>
        /// Access key for AWS credentials
        /// </summary>
        public string AccessKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Secret key for AWS credentials
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Session token for STS credentials
        /// </summary>
        public string SessionToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional credential properties for custom implementations
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// AWS-specific connection options
    /// </summary>
    public class AwsConnectionOptions
    {
        /// <summary>
        /// AWS region
        /// </summary>
        public string Region { get; set; } = "us-east-1";
        
        /// <summary>
        /// AWS credentials configuration
        /// </summary>
        public CredentialsOptions Credentials { get; set; } = new CredentialsOptions();
        
        /// <summary>
        /// Retry configuration
        /// </summary>
        public RetryOptions Retry { get; set; } = new RetryOptions();
        
        /// <summary>
        /// Additional AWS-specific properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        
        // Backward compatibility properties - these delegate to Credentials
        /// <summary>
        /// Access key for AWS credentials (backward compatibility)
        /// </summary>
        public string AccessKey
        {
            get => Credentials.AccessKey;
            set => Credentials.AccessKey = value;
        }
        
        /// <summary>
        /// Secret key for AWS credentials (backward compatibility)
        /// </summary>
        public string SecretKey
        {
            get => Credentials.SecretKey;
            set => Credentials.SecretKey = value;
        }
        
        /// <summary>
        /// Session token for STS credentials
        /// </summary>
        public string SessionToken
        {
            get => Credentials.SessionToken;
            set => Credentials.SessionToken = value;
        }
    }
}