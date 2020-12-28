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
		private readonly ServerEndPoint _endpoint;

		public EndPoint EndPoint => throw new NotImplementedException();

		public ServerConnectionManager(ILoggerFactory loggerFactory, ServerEndPoint endpoint)
		{
			_loggerFactory = loggerFactory;
			_endpoint = endpoint;
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
