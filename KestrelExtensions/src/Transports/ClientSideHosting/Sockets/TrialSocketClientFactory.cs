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

		public ValueTask<TransportFactoryClientResult> TryCreateClientAsync(ServerEndPoint endpoint, CancellationToken cancellationToken = default)
		{
			var underlyingEndpoint = endpoint.EndPoint;
			if (underlyingEndpoint is FileHandleEndPoint ||
				underlyingEndpoint is UnixDomainSocketEndPoint ||
				(underlyingEndpoint is IPEndPoint ipEndPoint &&
				ipEndPoint.Port != 0)) //  a port of 0 indicates an extension endpoint masquerading as an IPEndPoint to avoid Kestrel throwing exceptions
			{
				return ValueTask.FromResult(new TransportFactoryClientResult(
					true, endpoint, new SocketClient(endpoint, _loggerFactory)
					));
			}

			return ValueTask.FromResult(new TransportFactoryClientResult(endpoint));
		}
	}
}
