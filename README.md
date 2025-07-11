# Secret Service for .NET Standard

A modern secret service library for .NET Standard applications that provides unified access to AWS secrets via Parameter Store and Secrets Manager.

## Features

- **AWS Parameter Store Integration**: Built on AWS SDK for .NET for reliable Parameter Store connectivity
- **AWS Secrets Manager Integration**: Built on AWS SDK for .NET for reliable Secrets Manager connectivity
- **Automatic Caching**: Built-in 10-minute caching to reduce AWS API calls and improve performance
- **Automatic Retry**: Configurable retry policies for AWS service exceptions (default: 3 retries)
- **Multiple Authentication**: Support for IAM roles and access key/secret key authentication
- **Dependency Injection Ready**: Full support for modern .NET DI containers with provider pattern
- **Easy Configuration**: Support for appsettings.json and manual configuration
- **Thread Safety**: All operations are thread-safe with proper locking mechanisms

## Important Notes

- All secret values are automatically cached for 10 minutes to reduce AWS API calls
- The library manages automatic cache expiration and refresh
- Both IAM role-based and credential-based authentication are supported
- AWS SDK v4 is used for optimal performance and latest features

## Usage

### Provider Pattern with Dependency Injection

The secret service supports multiple configuration approaches, including configuration from appsettings.json files.

#### Configuration from appsettings.json

**appsettings.json example for Parameter Store:**

```json
{
  "SecretService": {
    "DefaultProvider": "ParamStore",
    "ParamStore": {
      "Region": "us-east-1",
      "AccessKey": "",
      "SecretKey": "",
      "Retry": {
        "MaxRetries": 3,
        "DelaySeconds": 2,
        "Enabled": true
      }
    }
  }
}
```

**appsettings.json example for Secrets Manager:**

```json
{
  "SecretService": {
    "DefaultProvider": "SecretsManager",
    "SecretsManager": {
      "Region": "us-west-2",
      "AccessKey": "",
      "SecretKey": "",
      "Retry": {
        "MaxRetries": 5,
        "DelaySeconds": 1,
        "Enabled": true
      }
    }
  }
}
```

**Production appsettings.json with long-term credentials:**

```json
{
  "SecretService": {
    "DefaultProvider": "SecretsManager",
    "SecretsManager": {
      "Region": "us-west-2",
      "AccessKey": "AKIA...",
      "SecretKey": "...",
      "Retry": {
        "MaxRetries": 3,
        "DelaySeconds": 2,
        "Enabled": true
      }
    }
  }
}
```

**Production appsettings.json with STS (temporary) credentials:**

```json
{
  "SecretService": {
    "DefaultProvider": "ParamStore",
    "ParamStore": {
      "Region": "us-east-1",
      "Credentials": {
        "AuthenticationType": "STS",
        "AccessKey": "AKIA...",
        "SecretKey": "...",
        "SessionToken": "IQoJb3JpZ2luX2VjE..."
      },
      "Retry": {
        "MaxRetries": 3,
        "DelaySeconds": 2,
        "Enabled": true
      }
    }
  }
}
```

#### Console Application with appsettings.json

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Security.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                          optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Configure secret service from appsettings.json
        services.AddSecretService(context.Configuration);

        // Register your application services
        services.AddScoped<MyService>();
    })
    .Build();

// Use the secret service
var myService = host.Services.GetRequiredService<MyService>();
await myService.DoSomethingWithSecrets();

public class MyService
{
    private readonly ISecrets _secrets;

    public MyService(ISecrets secrets)
    {
        _secrets = secrets;
    }

    public async Task DoSomethingWithSecrets()
    {
        var dbPassword = await _secrets.GetSecretAsync("database/password");
        var apiKey = await _secrets.GetSecretAsync("external-api/key");
        
        Console.WriteLine("Retrieved secrets successfully");
        // Use secrets for database connection, API calls, etc.
    }

    public async Task DoSomethingWithSecretsAndCancellation(CancellationToken cancellationToken)
    {
        var dbPassword = await _secrets.GetSecretAsync("database/password", cancellationToken);
        var apiKey = await _secrets.GetSecretAsync("external-api/key", cancellationToken);
        
        Console.WriteLine("Retrieved secrets successfully with cancellation support");
        // Use secrets for database connection, API calls, etc.
    }
}
```

#### Console Application with Parameter Store

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Security.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Configure Parameter Store service from appsettings.json
        services.AddSecretService(context.Configuration);

        services.AddScoped<MyService>();
    })
    .Build();

var myService = host.Services.GetRequiredService<MyService>();
await myService.LoadParameterStoreSecrets();

public class MyService
{
    private readonly ISecrets _secrets;

    public MyService(ISecrets secrets)
    {
        _secrets = secrets;
    }

    public async Task LoadParameterStoreSecrets()
    {
        // Parameter Store uses path-like keys
        var dbConnectionString = await _secrets.GetSecretAsync("/myapp/database/connectionstring");
        var emailApiKey = await _secrets.GetSecretAsync("/myapp/email/apikey");
        var cacheEndpoint = await _secrets.GetSecretAsync("/myapp/cache/endpoint");
        
        Console.WriteLine("Parameter Store secrets loaded successfully");
        // Configure services with retrieved parameters
    }
}
```

#### ASP.NET Core Application with appsettings.json

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configure secret service from appsettings.json (uses "SecretService" section by default)
builder.Services.AddSecretService(builder.Configuration);

// Or specify a custom section name
// builder.Services.AddSecretService(builder.Configuration, "MyCustomSecretSection");

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();

// Controller
[ApiController]
[Route("api/[controller]")]
public class SecretsController : ControllerBase
{
    private readonly ISecrets _secrets;

    public SecretsController(ISecrets secrets)
    {
        _secrets = secrets;
    }

    [HttpGet("database-config")]
    public async Task<IActionResult> GetDatabaseConfig(CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = await _secrets.GetSecretAsync("database/connectionstring", cancellationToken);
            // Return sanitized config (never return actual secrets)
            return Ok(new { Status = "Connected", Provider = "Database" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Failed to retrieve configuration" });
        }
    }

    [HttpGet("api-status")]
    public async Task<IActionResult> GetApiStatus(CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = await _secrets.GetSecretAsync("external-api/key", cancellationToken);
            // Use API key to check external service status
            return Ok(new { Status = "External API configured" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Failed to retrieve API configuration" });
        }
    }
}
```

#### Provider Comparison and When to Use Each

**Parameter Store Features:**
- ✅ Simple string values with hierarchical paths
- ✅ Integration with AWS Systems Manager
- ✅ Built-in encryption with AWS KMS
- ✅ Cost-effective for simple configuration values
- ✅ Supports parameter versioning and history

**Secrets Manager Features:**
- ✅ JSON and complex secret values
- ✅ Automatic secret rotation
- ✅ Fine-grained access control
- ✅ Cross-region replication
- ✅ Integration with RDS, DocumentDB, Redshift

**When to Use Parameter Store:**
- Application configuration values
- Environment-specific settings
- Simple string secrets
- Cost-sensitive scenarios
- Hierarchical configuration management

**When to Use Secrets Manager:**
- Database passwords and connection strings
- API keys and tokens
- Certificates and keys
- Secrets requiring automatic rotation
- Complex JSON secret structures

#### CancellationToken Support

All secret retrieval methods support `CancellationToken` for request cancellation. This is especially useful for:

**Web Applications:**
```csharp
public async Task<string> GetConfigurationAsync(CancellationToken cancellationToken)
{
    // Automatically cancelled if HTTP request is cancelled
    return await _secrets.GetSecretAsync("app/config", cancellationToken);
}
```

**Background Services with Timeouts:**
```csharp
public async Task ProcessDataAsync()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    try
    {
        var apiKey = await _secrets.GetSecretAsync("external-api/key", cts.Token);
        // Process with 30-second timeout
    }
    catch (OperationCanceledException)
    {
        // Handle timeout
        throw new TimeoutException("Secret retrieval timed out after 30 seconds");
    }
}
```

**Graceful Shutdown:**
```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    // Service startup with cancellation support
    var dbConnection = await _secrets.GetSecretAsync("database/connection", cancellationToken);
    // Configure services...
}
```

**Benefits of CancellationToken:**
- ✅ Prevents unnecessary AWS API calls when operations are cancelled
- ✅ Improves application responsiveness in web scenarios
- ✅ Supports timeout scenarios and graceful shutdown
- ✅ Standard .NET async pattern compliance
- ✅ Default parameter - existing code continues to work

#### Provider Pattern without Dependency Injection (.NET Framework Compatible)

For .NET Framework applications or scenarios where you need to use the secret providers without dependency injection:

**Parameter Store Provider Example:**

```csharp
using Security.Configuration;
using Security.Providers;

// Create Parameter Store provider with configuration
var paramStoreOptions = new ParamStoreOptions
{
    Region = "us-east-1",
    AccessKey = "", // Leave empty to use IAM role
    SecretKey = "", // Leave empty to use IAM role
    Retry = new RetryOptions
    {
        MaxRetries = 3,
        DelaySeconds = 2,
        Enabled = true
    }
};

// Create provider and service instance
var provider = new ParamStoreProvider(paramStoreOptions);
var secrets = provider.CreateService();

// Use the service
var dbPassword = await secrets.GetSecretAsync("/myapp/database/password");
var apiKey = await secrets.GetSecretAsync("/myapp/external/apikey");
Console.WriteLine("Retrieved Parameter Store secrets successfully");
```

**Secrets Manager Provider Example:**

```csharp
using Security.Configuration;
using Security.Providers;

// Create Secrets Manager provider with configuration
var secretsManagerOptions = new SecretsManagerOptions
{
    Region = "us-west-2",
    AccessKey = "AKIA...", // Provide credentials or leave empty for IAM role
    SecretKey = "...",     // Provide credentials or leave empty for IAM role
    Retry = new RetryOptions
    {
        MaxRetries = 5,
        DelaySeconds = 1,
        Enabled = true
    }
};

// Create provider and service instance
var provider = new SecretsManagerProvider(secretsManagerOptions);
var secrets = provider.CreateService();

// Use the service
var dbConnectionString = await secrets.GetSecretAsync("prod/database/connectionstring");
var jwtSecret = await secrets.GetSecretAsync("prod/auth/jwt-secret");
Console.WriteLine("Retrieved Secrets Manager secrets successfully");
```

**Using IAM Role Authentication (Recommended):**

```csharp
using Security.Configuration;
using Security.Providers;

// Parameter Store with IAM role (no credentials needed)
var paramStoreOptions = new ParamStoreOptions
{
    Region = "us-east-1"
    // AccessKey and SecretKey are empty - will use IAM role
};

var paramStoreProvider = new ParamStoreProvider(paramStoreOptions);
var paramStoreSecrets = paramStoreProvider.CreateService();

// Secrets Manager with IAM role (no credentials needed)
var secretsManagerOptions = new SecretsManagerOptions
{
    Region = "us-east-1"
    // AccessKey and SecretKey are empty - will use IAM role
};

var secretsManagerProvider = new SecretsManagerProvider(secretsManagerOptions);
var secretsManagerSecrets = secretsManagerProvider.CreateService();

// Use both services
var configValue = await paramStoreSecrets.GetSecretAsync("/app/config/setting");
var dbPassword = await secretsManagerSecrets.GetSecretAsync("database/password");
```

**Using STS (Temporary) Credentials:**

```csharp
using Security.Configuration;
using Security.Providers;

// Parameter Store with STS credentials
var paramStoreOptions = new ParamStoreOptions
{
    Region = "us-east-1",
    Credentials = new CredentialsOptions
    {
        AuthenticationType = "STS",
        AccessKey = "AKIA...",
        SecretKey = "...",
        SessionToken = "IQoJb3JpZ2luX2VjE..."
    }
};

// Or use backward-compatible properties
var paramStoreOptionsCompat = new ParamStoreOptions
{
    Region = "us-east-1",
    AccessKey = "AKIA...",
    SecretKey = "...",
    SessionToken = "IQoJb3JpZ2luX2VjE..."
};

var provider = new ParamStoreProvider(paramStoreOptions);
var secrets = provider.CreateService();

// Use the service - STS credentials will be automatically handled
var configValue = await secrets.GetSecretAsync("/app/database/password");
var apiKey = await secrets.GetSecretAsync("/app/external/apikey");

// With cancellation token for timeout scenarios
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var timeoutConfigValue = await secrets.GetSecretAsync("/app/database/password", cts.Token);
```

**Direct Service Usage (Alternative Pattern):**

```csharp
using Security;
using Security.Configuration;

// Create Parameter Store service directly
var paramStoreOptions = new ParamStoreOptions
{
    Region = "us-west-2",
    Retry = new RetryOptions { MaxRetries = 5 }
};

var paramStore = new ParamStore(paramStoreOptions);
var configValue = await paramStore.GetSecretAsync("/app/feature/enabled");

// Create Secrets Manager service directly
var secretsManagerOptions = new SecretsManagerOptions
{
    Region = "us-west-2",
    AccessKey = "AKIA...",
    SecretKey = "..."
};

var secretsManager = new SecretsManager(secretsManagerOptions);
var apiSecret = await secretsManager.GetSecretAsync("prod/api/secret");
```

#### Manual Configuration (Without appsettings.json)

```csharp
// Option 1: Use configuration action for Parameter Store
services.AddSecretService(options =>
{
    options.DefaultProvider = "ParamStore";
    options.ParamStore.Region = "us-east-1";
    options.ParamStore.Retry.MaxRetries = 5;
    options.ParamStore.Retry.DelaySeconds = 1;
});

// Option 2: Use configuration object for Secrets Manager
var secretServiceOptions = new SecretServiceOptions
{
    DefaultProvider = "SecretsManager",
    SecretsManager = new SecretsManagerOptions
    {
        Region = "us-west-2",
        AccessKey = "AKIA...",
        SecretKey = "...",
        Retry = new RetryOptions
        {
            MaxRetries = 3,
            DelaySeconds = 2,
            Enabled = true
        }
    }
};
services.AddSecretService(secretServiceOptions);

// Option 3: Parameter Store with credentials
services.AddSecretService(options =>
{
    options.DefaultProvider = "ParamStore";
    options.ParamStore.Region = "us-east-1";
    options.ParamStore.AccessKey = "AKIA...";
    options.ParamStore.SecretKey = "...";
    options.ParamStore.Retry.MaxRetries = 3;
    options.ParamStore.Retry.DelaySeconds = 2;
});

// Option 4: IAM Role configuration (recommended)
services.AddSecretService(options =>
{
    options.DefaultProvider = "SecretsManager";
    options.SecretsManager.Region = "us-east-1";
    // No AccessKey/SecretKey - uses IAM role
    options.SecretsManager.Retry.MaxRetries = 3;
});
```

## Authentication Methods

### IAM Role Authentication (Recommended)

The recommended approach is to use IAM roles, especially when running on AWS infrastructure:

- **EC2 Instance**: Attach IAM role to EC2 instance
- **ECS Task**: Assign IAM role to ECS task
- **Lambda Function**: Lambda execution role
- **Local Development**: Use AWS CLI profiles or AWS credentials file

```csharp
// No credentials needed - automatically uses IAM role
var options = new ParamStoreOptions
{
    Region = "us-east-1"
};
```

### STS (Temporary) Credentials

For scenarios using AWS Security Token Service (STS) for temporary credentials:

```csharp
// Modern approach using Credentials object
var options = new SecretsManagerOptions
{
    Region = "us-west-2",
    Credentials = new CredentialsOptions
    {
        AuthenticationType = "STS",
        AccessKey = "AKIA...",
        SecretKey = "...",
        SessionToken = "IQoJb3JpZ2luX2VjE..."
    }
};

// Backward-compatible approach
var optionsCompat = new SecretsManagerOptions
{
    Region = "us-west-2",
    AccessKey = "AKIA...",
    SecretKey = "...",
    SessionToken = "IQoJb3JpZ2luX2VjE..."
};
```

**When to use STS credentials:**
- Cross-account access with assume role
- Temporary access for applications
- MFA-protected operations
- Fed identity scenarios (SAML, OIDC)
- CI/CD pipelines with temporary access

### Long-term Access Key Authentication

For environments where IAM roles aren't available:

```csharp
var options = new SecretsManagerOptions
{
    Region = "us-west-2",
    AccessKey = "AKIA...",
    SecretKey = "..."
};
```

### Custom Authentication (Future Extensibility)

The credential system is designed to support future non-AWS implementations:

```csharp
var credentials = new CredentialsOptions
{
    AuthenticationType = "Custom",
    Properties = new Dictionary<string, string>
    {
        { "endpoint", "https://vault.company.com" },
        { "token", "hvs.CAESIJK..." },
        { "namespace", "admin/dev" }
    }
};
```

**Security Best Practices:**
- Store credentials in environment variables, not in code
- Use IAM roles whenever possible
- Prefer STS over long-term credentials for temporary access
- Apply principle of least privilege
- Rotate credentials regularly
- Never commit credentials to source control
- Use shortest possible session duration for STS tokens

## Configuration Reference

### SecretServiceOptions

```csharp
public class SecretServiceOptions
{
    public string DefaultProvider { get; set; } = "ParamStore"; // "ParamStore" or "SecretsManager"
    public ParamStoreOptions ParamStore { get; set; } = new ParamStoreOptions();
    public SecretsManagerOptions SecretsManager { get; set; } = new SecretsManagerOptions();
}
```

### ParamStoreOptions (inherits from AwsConnectionOptions)

```csharp
public class ParamStoreOptions : AwsConnectionOptions
{
    // Inherits all properties from AwsConnectionOptions
    // Including Region, Credentials, Retry, and Properties
}
```

### SecretsManagerOptions (inherits from AwsConnectionOptions)

```csharp
public class SecretsManagerOptions : AwsConnectionOptions
{
    // Inherits all properties from AwsConnectionOptions
    // Including Region, Credentials, Retry, and Properties
}
```

### AwsConnectionOptions

```csharp
public class AwsConnectionOptions
{
    public string Region { get; set; } = "us-east-1";                    // AWS region
    public CredentialsOptions Credentials { get; set; } = new CredentialsOptions(); // Modern credentials
    public RetryOptions Retry { get; set; } = new RetryOptions();        // Retry configuration
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>(); // Additional properties
    
    // Backward compatibility properties (delegate to Credentials)
    public string AccessKey { get; set; }      // Maps to Credentials.AccessKey
    public string SecretKey { get; set; }      // Maps to Credentials.SecretKey  
    public string SessionToken { get; set; }   // Maps to Credentials.SessionToken
}
```

### CredentialsOptions

```csharp
public class CredentialsOptions
{
    public string AuthenticationType { get; set; } = "None";     // "None", "AccessKey", "STS", "Custom"
    public string AccessKey { get; set; } = string.Empty;        // AWS access key
    public string SecretKey { get; set; } = string.Empty;        // AWS secret key
    public string SessionToken { get; set; } = string.Empty;     // STS session token
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>(); // Custom properties
}
```

### RetryOptions

```csharp
public class RetryOptions
{
    public int MaxRetries { get; set; } = 3;        // Number of retry attempts
    public int DelaySeconds { get; set; } = 2;      // Delay between retries
    public bool Enabled { get; set; } = true;      // Enable/disable retry
}
```

## Caching Behavior

The secret service automatically caches retrieved secrets for **10 minutes** to improve performance and reduce AWS API calls.

### Cache Features

- **Automatic Expiration**: Secrets are automatically refreshed after 10 minutes
- **Thread Safety**: Cache operations are thread-safe with proper locking
- **Memory Efficient**: Only stores string values, not entire AWS response objects
- **Per-Key Caching**: Each secret key is cached independently

### Cache Behavior

```csharp
// First call - retrieves from AWS and caches
var secret1 = await secrets.GetSecretAsync("database/password");

// Second call - returns from cache (within 10 minutes)
var secret2 = await secrets.GetSecretAsync("database/password");

// After 10 minutes - retrieves from AWS again and updates cache
var secret3 = await secrets.GetSecretAsync("database/password");
```

### Testing Cache Behavior

For testing purposes, you can modify the cache expiration time:

```csharp
// Only use in tests - modifies global cache behavior
ParamStore.SetCacheRefreshTimeTo(1); // 1 second for testing
SecretsManager.SetCacheRefreshTimeTo(1); // 1 second for testing
```

## Error Handling

The secret service includes automatic retry logic for transient failures and provides custom exceptions that abstract AWS-specific errors. **Consumers do not need to reference AWS SDK packages to handle exceptions.**

### Custom Exception Hierarchy

All secret service exceptions inherit from `SecretServiceException`:

```csharp
using Security.Exceptions;

try
{
    var secret = await secrets.GetSecretAsync("database/password");
    // Use secret
}
catch (SecretNotFoundException ex)
{
    // Secret not found in Parameter Store or Secrets Manager
    Console.WriteLine($"Secret '{ex.SecretKey}' does not exist");
}
catch (SecretAccessDeniedException ex)
{
    // Access denied due to insufficient IAM permissions
    Console.WriteLine($"Access denied to secret '{ex.SecretKey}'. Check IAM permissions.");
}
catch (SecretAuthenticationException ex)
{
    // Authentication failed (invalid credentials, expired tokens, etc.)
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (SecretConfigurationException ex)
{
    // Invalid configuration (invalid region, malformed secret name, etc.)
    Console.WriteLine($"Configuration error: {ex.Message}");
}
catch (SecretServiceUnavailableException ex)
{
    // Service temporarily unavailable (rate limiting, service outage)
    Console.WriteLine($"Service unavailable: {ex.Message}");
}
catch (SecretTimeoutException ex)
{
    // Request timed out
    Console.WriteLine($"Request timed out: {ex.Message}");
}
catch (SecretServiceInternalException ex)
{
    // Unexpected internal errors
    Console.WriteLine($"Internal error: {ex.Message}");
}
catch (ArgumentNullException)
{
    // Null or empty secret key provided
    Console.WriteLine("Secret key cannot be null or empty");
}
```

### Exception Types

| Exception | Description | When Thrown |
|-----------|-------------|-------------|
| `SecretNotFoundException` | Secret key not found | Parameter/secret doesn't exist |
| `SecretAccessDeniedException` | Access denied | Insufficient IAM permissions, decryption failures |
| `SecretAuthenticationException` | Authentication failed | Invalid credentials, expired STS tokens |
| `SecretConfigurationException` | Invalid configuration | Bad region, malformed secret names |
| `SecretServiceUnavailableException` | Service unavailable | Rate limiting, AWS service outages |
| `SecretTimeoutException` | Request timeout | Network timeouts, slow responses |
| `SecretServiceInternalException` | Unexpected errors | Other AWS SDK exceptions |

### Practical Error Handling

```csharp
public async Task<string> GetDatabaseConnectionString()
{
    try
    {
        return await _secrets.GetSecretAsync("database/connectionstring");
    }
    catch (SecretNotFoundException)
    {
        // Handle missing secret - maybe return default or create it
        throw new InvalidOperationException("Database connection string not configured");
    }
    catch (SecretAccessDeniedException)
    {
        // Handle permission issues
        throw new UnauthorizedAccessException("Application lacks permission to access database secrets");
    }
    catch (SecretServiceException ex)
    {
        // Handle all other secret service errors
        throw new ApplicationException($"Failed to retrieve database connection: {ex.Message}", ex);
    }
}
```

### Benefits of Custom Exceptions

- **No AWS SDK Dependency**: Consumers don't need AWS SDK references for exception handling
- **Provider Agnostic**: Same exceptions work for Parameter Store, Secrets Manager, and future providers
- **Rich Context**: Exceptions include relevant information like secret keys
- **Consistent Behavior**: Same exception types across all providers
- **Inner Exception Preservation**: Original AWS exceptions preserved for debugging

## Performance Considerations

- **Caching**: 10-minute cache reduces AWS API calls by 99%+ for frequently accessed secrets
- **Connection Reuse**: AWS SDK automatically manages connection pooling
- **Retry Logic**: Exponential backoff prevents overwhelming AWS services during failures
- **Thread Safety**: All operations are thread-safe and can be used concurrently

## AWS Permissions

### Parameter Store Permissions

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "ssm:GetParameter",
                "ssm:GetParameters"
            ],
            "Resource": "arn:aws:ssm:region:account:parameter/your-app/*"
        },
        {
            "Effect": "Allow",
            "Action": [
                "kms:Decrypt"
            ],
            "Resource": "arn:aws:kms:region:account:key/your-key-id"
        }
    ]
}
```

### Secrets Manager Permissions

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "secretsmanager:GetSecretValue"
            ],
            "Resource": "arn:aws:secretsmanager:region:account:secret:your-secret-*"
        }
    ]
}
```

## Release Notes

### 2.0.0
- **BREAKING CHANGE**: Migrated from factory pattern to modern provider pattern
- **BREAKING CHANGE**: Removed factory classes (`ParamStoreFactory`, `SecretsManagerFactory`)
- **NEW**: Custom exception hierarchy - consumers no longer need AWS SDK references for exception handling
- **NEW**: STS (temporary) credential support for cross-account access and temporary tokens
- **NEW**: Generic credential system supporting future non-AWS implementations
- **NEW**: Full dependency injection support with provider pattern
- **NEW**: Configuration from appsettings.json
- Updated to AWS SDK v4 (latest versions: SecretsManager 4.0.0.11, SimpleSystemsManagement 4.0.2.1)
- Improved thread safety and performance
- Added comprehensive test coverage (30 tests)
- Modern .NET patterns and practices
- Enhanced security with proper credential abstraction

### 1.2
- Updated `ParamStore` and `SecretsManager` to use internal caching
- Fetched values are stored locally for ten minutes before being pulled from AWS again

