using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public class TrialServerTransportFactory : ITrialConnectionListenerFactory
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly List<ITrialConnectionClientFactory> _clientFactories;

		public TrialServerTransportFactory(ILoggerFactory loggerFactory, IEnumerable<ITrialConnectionClientFactory> clientFactories)
		{
			_loggerFactory = loggerFactory;
			_clientFactories = clientFactories.ToList();
		}

		private async Task<IConnectionClient?> CreateConnectionClient(EndPoint endpoint, CancellationToken cancellationToken)
		{
			foreach (var factory in _clientFactories)
			{
				var result = await factory.TryCreateClientAsync(endpoint, cancellationToken);
				if (result.DidCreateClient)
				{
					return result.ConnectionClient;
				}
			}

			return null;
		}

		public async ValueTask<TransportFactoryBindResult> TryBindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
		{
			if (endpoint is ServerEndPoint serverEndPoint)
			{
				var connectionClient = await CreateConnectionClient(serverEndPoint.EndPoint, cancellationToken);
				if (connectionClient == null)
				{
					return new TransportFactoryBindResult(endpoint);
				}

				var listener = new ServerConnectionManager(_loggerFactory.CreateLogger<ServerConnectionManager>(),
					connectionClient);
				return new TransportFactoryBindResult(true, endpoint, listener);
			}

			return new TransportFactoryBindResult(endpoint);
		}
	}
}
