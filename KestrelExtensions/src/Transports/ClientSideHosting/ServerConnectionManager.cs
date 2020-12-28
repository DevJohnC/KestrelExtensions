using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	internal class ServerConnectionManager : IConnectionListener
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly IConnectionClient _client;

		public EndPoint EndPoint => _client.SpecifiedEndPoint;

		public ServerConnectionManager(ILoggerFactory loggerFactory, IConnectionClient client)
		{
			_loggerFactory = loggerFactory;
			_client = client;
		}

		public void Start()
		{

		}

		public ValueTask<ConnectionContext> AcceptAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}

		public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
