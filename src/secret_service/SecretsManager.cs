using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.Runtime;
using Security.Configuration;
using Security.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Security
{
    public sealed class SecretsManager : ISecrets
    {
        private readonly IAmazonSecretsManager _amazonClient;
        private readonly AmazonSecretsManagerConfig _amazonConfig;
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();
        private static int CacheRefreshTimeInSeconds = 600; // 10 minute default 
        private static DateTime CacheExpiresOn = DateTime.UtcNow.AddSeconds(CacheRefreshTimeInSeconds);

        public SecretsManager(SecretsManagerOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var credentials = CreateAwsCredentials(options.Credentials);
            _amazonConfig = CreateConfig(options.Region, options.Retry.MaxRetries);

            if (credentials == null)
            {
                _amazonClient = new AmazonSecretsManagerClient(_amazonConfig);
            }
            else
            {
                _amazonClient = new AmazonSecretsManagerClient(credentials, _amazonConfig);
            }
        }

        // used for unit testing
        [ExcludeFromCodeCoverage]
        internal SecretsManager(IAmazonSecretsManager client)
        {
            _amazonClient = client;
        }

        // allow override for testing
        internal static void SetCacheRefreshTimeTo(int seconds)
        {
            CacheRefreshTimeInSeconds = seconds;
            CacheExpiresOn = DateTime.MinValue;
        }

        private static AWSCredentials CreateAwsCredentials(CredentialsOptions credentialsOptions)
        {
            if (credentialsOptions == null)
                return null;

            // If no credentials specified, use default (IAM role, AWS CLI profiles, etc.)
            if (string.IsNullOrEmpty(credentialsOptions.AccessKey) && 
                string.IsNullOrEmpty(credentialsOptions.SecretKey) && 
                string.IsNullOrEmpty(credentialsOptions.SessionToken))
            {
                return null;
            }

            // STS credentials (temporary credentials with session token)
            if (!string.IsNullOrEmpty(credentialsOptions.SessionToken))
            {
                if (string.IsNullOrEmpty(credentialsOptions.AccessKey) || string.IsNullOrEmpty(credentialsOptions.SecretKey))
                    throw new ArgumentException("STS credentials require AccessKey, SecretKey, and SessionToken");

                return new SessionAWSCredentials(credentialsOptions.AccessKey, credentialsOptions.SecretKey, credentialsOptions.SessionToken);
            }

            // Long-term credentials (access key + secret key)
            if (!string.IsNullOrEmpty(credentialsOptions.AccessKey) && !string.IsNullOrEmpty(credentialsOptions.SecretKey))
            {
                return new BasicAWSCredentials(credentialsOptions.AccessKey, credentialsOptions.SecretKey);
            }

            // If only one credential component is provided, that's an error
            if (!string.IsNullOrEmpty(credentialsOptions.AccessKey) || !string.IsNullOrEmpty(credentialsOptions.SecretKey))
            {
                throw new ArgumentException("Both AccessKey and SecretKey must be provided for basic credentials");
            }

            return null;
        }

        private static AmazonSecretsManagerConfig CreateConfig(string region, int retries)
        {

            if (retries <= 0)
                throw new ArgumentException("retries must be greater than 0", nameof(retries));

            if (string.IsNullOrEmpty(region))
                throw new ArgumentNullException(nameof(region), "region is null or empty");

            return new AmazonSecretsManagerConfig
            {
                MaxErrorRetry = retries,
                UseHttp = false,
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            };
        }

        public async Task<string> GetSecretAsync(string secretKey, CancellationToken cancellationToken = default)
        {
            Helpers.ValidateSecretKey(secretKey);
            CheckCacheState();

            if (!Cache.TryGetValue(secretKey, out var value))
            {
                value = await GetSecretFromAws(secretKey, cancellationToken);

                lock (Cache)
                {
                    if (!Cache.ContainsKey(secretKey))
                    {
                        Cache.Add(secretKey, value);
                    }
                }
            }

            return value;
        }

        private static void CheckCacheState()
        {
            if (DateTime.UtcNow > CacheExpiresOn)
            {
                Cache.Clear();
                CacheExpiresOn = DateTime.UtcNow.AddSeconds(CacheRefreshTimeInSeconds);
            }
        }

        private async Task<string> GetSecretFromAws(string key, CancellationToken cancellationToken)
        {
            try
            {
                var request = new GetSecretValueRequest()
                {
                    SecretId = key
                };

                var response = await _amazonClient.GetSecretValueAsync(request, cancellationToken);
                return response.SecretString;
            }
            catch (ResourceNotFoundException ex)
            {
                throw new SecretNotFoundException(key, ex);
            }
            catch (InvalidParameterException ex)
            {
                throw new SecretConfigurationException($"Invalid parameter for secret '{key}': {ex.Message}", ex);
            }
            catch (InvalidRequestException ex)
            {
                throw new SecretConfigurationException($"Invalid request for secret '{key}': {ex.Message}", ex);
            }
            catch (DecryptionFailureException ex)
            {
                throw new SecretAccessDeniedException(key, ex);
            }
            catch (InternalServiceErrorException ex)
            {
                throw new SecretServiceInternalException($"Secrets Manager internal error for '{key}': {ex.Message}", ex);
            }
            catch (AmazonSecretsManagerException ex) when (ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            {
                throw new SecretTimeoutException($"Secrets Manager request timed out for '{key}': {ex.Message}", ex);
            }
            catch (AmazonSecretsManagerException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                throw new SecretServiceUnavailableException($"Secrets Manager service unavailable: {ex.Message}", ex);
            }
            catch (AmazonSecretsManagerException ex)
            {
                throw new SecretServiceInternalException($"Secrets Manager error for '{key}': {ex.Message}", ex);
            }
            catch (AmazonServiceException ex)
            {
                throw new SecretServiceInternalException($"AWS service error for '{key}': {ex.Message}", ex);
            }
            catch (AmazonClientException ex)
            {
                throw new SecretServiceInternalException($"AWS client error for '{key}': {ex.Message}", ex);
            }
        }
    }
}