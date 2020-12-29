using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting.Sockets
{
	public class TrialSocketClientFactory : ITrialConnectionClientFactory
	{
		private readonly ILoggerFactory _loggerFactory;

		public TrialSocketClientFactory(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		public async ValueTask<TransportFactoryClientResult> TryCreateClientAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
		{
			if (endpoint is FileHandleEndPoint ||
				endpoint is UnixDomainSocketEndPoint ||
				(endpoint is IPEndPoint ipEndPoint &&
				ipEndPoint.Port != 0)) //  a port of 0 indicates an extension endpoint masquerading as an IPEndPoint to avoid Kestrel throwing exceptions
			{
				return new TransportFactoryClientResult(
					true, endpoint, new SocketClient(endpoint, _loggerFactory)
					);
			}

			return new TransportFactoryClientResult(endpoint);
		}
	}
}
