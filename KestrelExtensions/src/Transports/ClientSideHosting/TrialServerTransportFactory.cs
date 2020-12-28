using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public class TrialServerTransportFactory : ITrialConnectionListenerFactory
	{
		private readonly ILoggerFactory _loggerFactory;

		public TrialServerTransportFactory(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		public ValueTask<TransportFactoryBindResult> TryBindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
		{
			if (endpoint is ServerEndPoint serverEndPoint)
			{
				var listener = new ServerConnectionManager(_loggerFactory, serverEndPoint);
				listener.Start();
				return new ValueTask<TransportFactoryBindResult>(
					new TransportFactoryBindResult(true, endpoint, listener)
					);
			}

			return new ValueTask<TransportFactoryBindResult>(
				new TransportFactoryBindResult(endpoint)
				);
		}
	}
}
