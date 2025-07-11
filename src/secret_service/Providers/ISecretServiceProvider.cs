namespace Security.Providers
{
    public interface ISecretServiceProvider
    {
        ISecrets CreateService();
    }
}