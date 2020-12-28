using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.Sockets
{
	/// <summary>
	/// Trial factory for Kestrel's <see cref="SocketTransportFactory"/> that supports
	/// <see cref="IPEndPoint"/>, <see cref="UnixDomainSocketEndPoint"/> and <see cref="FileHandleEndPoint"/>.
	/// </summary>
	public class TrialSocketTransportFactory : ITrialConnectionListenerFactory
	{
		private readonly SocketTransportFactory _factory;

		public TrialSocketTransportFactory(SocketTransportFactory factory)
		{
			_factory = factory;
		}

		public async ValueTask<TransportFactoryBindResult> TryBindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
		{
			if (endpoint is FileHandleEndPoint ||
				endpoint is UnixDomainSocketEndPoint ||
				(endpoint is IPEndPoint ipEndPoint &&
				ipEndPoint.Port != 0)) //  a port of 0 indicates an extension endpoint masquerading as an IPEndPoint to avoid Kestrel throwing exceptions
			{
				return new TransportFactoryBindResult(
					true, endpoint, await _factory.BindAsync(endpoint, cancellationToken)
					);
			}

			return new TransportFactoryBindResult(endpoint);
		}
	}
}
