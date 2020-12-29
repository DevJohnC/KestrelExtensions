using KestrelExtensions.Transports.Pipes;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting.Pipes
{
	public class TrialPipeClientFactory : ITrialConnectionClientFactory
	{
		private readonly ILoggerFactory _loggerFactory;

		public TrialPipeClientFactory(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		public ValueTask<TransportFactoryClientResult> TryCreateClientAsync(ServerEndPoint endpoint, CancellationToken cancellationToken = default)
		{
			var underlyingEndpoint = endpoint.EndPoint;
			if (underlyingEndpoint is NamedPipeEndPoint)
			{
				return ValueTask.FromResult(new TransportFactoryClientResult(
					true, endpoint, new PipeClient(endpoint, _loggerFactory)
					));
			}

			return ValueTask.FromResult(new TransportFactoryClientResult(endpoint));
		}
	}
}
