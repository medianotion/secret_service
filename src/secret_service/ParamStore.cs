using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
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
    public class ParamStore : ISecrets
    {
        private readonly IAmazonSimpleSystemsManagement _amazonClient;
        private static int CacheRefreshTimeInSeconds = 600; // 10 minute default 
        private static DateTime CacheExpiresOn = DateTime.UtcNow.AddSeconds(CacheRefreshTimeInSeconds);
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        public ParamStore(ParamStoreOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var credentials = CreateAwsCredentials(options.Credentials);
            var config = CreateConfig(options.Region, options.Retry.MaxRetries);

            if (credentials == null)
            {
                _amazonClient = new AmazonSimpleSystemsManagementClient(config);
            }
            else
            {
                _amazonClient = new AmazonSimpleSystemsManagementClient(credentials, config);
            }
        }

        // allow override for testing
        internal static void SetCacheRefreshTimeTo(int seconds)
        {
            CacheRefreshTimeInSeconds = seconds;
            CacheExpiresOn = DateTime.MinValue;
        }

        // used for unit testing
        [ExcludeFromCodeCoverage]
        internal ParamStore(IAmazonSimpleSystemsManagement client)
        {
            _amazonClient = client;
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

        private static AmazonSimpleSystemsManagementConfig CreateConfig(string region, int retries)
        {

            if (retries <= 0)
                throw new ArgumentException("retries must be greater than 0", nameof(retries));

            if (string.IsNullOrEmpty(region))
                throw new ArgumentNullException(nameof(region), "region is null or empty");

            return new AmazonSimpleSystemsManagementConfig
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
                var request = new GetParameterRequest
                {
                    Name = key,
                    WithDecryption = true
                };

                var response = await _amazonClient.GetParameterAsync(request, cancellationToken);
                return response.Parameter.Value;
            }
            catch (ParameterNotFoundException ex)
            {
                throw new SecretNotFoundException(key, ex);
            }
            catch (ParameterVersionNotFoundException ex)
            {
                throw new SecretNotFoundException(key, ex);
            }
            catch (TooManyUpdatesException ex)
            {
                throw new SecretServiceUnavailableException($"Parameter Store rate limit exceeded: {ex.Message}", ex);
            }
            catch (AmazonSimpleSystemsManagementException ex) when (ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            {
                throw new SecretTimeoutException($"Parameter Store request timed out for '{key}': {ex.Message}", ex);
            }
            catch (AmazonSimpleSystemsManagementException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                throw new SecretServiceUnavailableException($"Parameter Store service unavailable: {ex.Message}", ex);
            }
            catch (AmazonSimpleSystemsManagementException ex)
            {
                throw new SecretServiceInternalException($"Parameter Store error for '{key}': {ex.Message}", ex);
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