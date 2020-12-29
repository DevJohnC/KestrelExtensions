using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public interface ITrialConnectionClientFactory
	{
		ValueTask<TransportFactoryClientResult> TryCreateClientAsync(ServerEndPoint endpoint, CancellationToken cancellationToken = default);
	}
}
