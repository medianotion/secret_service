using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("unittest")]
namespace Security
{
	
	public interface ISecrets 
	{
		
		Task<string> GetSecretAsync(string secretKey, CancellationToken cancellationToken = default);
	}
}