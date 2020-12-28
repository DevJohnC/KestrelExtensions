using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.Pipes
{
	public class TrialPipesTransportFactory : ITrialConnectionListenerFactory
	{
		private readonly ILoggerFactory _loggerFactory;

		public TrialPipesTransportFactory(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		public ValueTask<TransportFactoryBindResult> TryBindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
		{
			if (endpoint is NamedPipeEndPoint namedPipeEndpoint)
			{
				var listener = new PipeConnectionListener(_loggerFactory, namedPipeEndpoint);
				listener.Bind();
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
