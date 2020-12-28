using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public interface ITrialConnectionClientFactory
	{
		ValueTask<TransportFactoryClientResult> TryCreateClientAsync(EndPoint endpoint, CancellationToken cancellationToken = default);
	}
}
