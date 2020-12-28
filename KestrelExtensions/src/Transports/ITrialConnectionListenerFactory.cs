using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports
{
	public interface ITrialConnectionListenerFactory
	{
		ValueTask<TransportFactoryBindResult> TryBindAsync(EndPoint endpoint, CancellationToken cancellationToken = default);
	}
}
