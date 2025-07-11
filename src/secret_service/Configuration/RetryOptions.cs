namespace Security.Configuration
{
    public class RetryOptions
    {
        public int MaxRetries { get; set; } = 3;
        public int DelaySeconds { get; set; } = 2;
        public bool Enabled { get; set; } = true;
    }
}