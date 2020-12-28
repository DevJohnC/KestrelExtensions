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
		private readonly IConnectionClient _client;
		private readonly ILogger<ServerConnectionManager> _logger;

		private TaskCompletionSource? _connectionClosed;
		private ConnectionContext? _currentConnection;

		public ServerConnectionManager(ILogger<ServerConnectionManager> logger, IConnectionClient client)
		{
			_logger = logger;
			_client = client;
		}

		public EndPoint EndPoint => _client.SpecifiedEndPoint;

		public async ValueTask<ConnectionContext> AcceptAsync(CancellationToken cancellationToken = default)
		{
			if (_currentConnection != null && _connectionClosed != null)
			{
				await _connectionClosed.Task;
			}

			var newContext = await _client.ConnectToServer(cancellationToken);
			var connectionClosed = new TaskCompletionSource();

			newContext.ConnectionClosed.Register(() =>
			{
				connectionClosed.SetResult();
			});

			_currentConnection = newContext;
			_connectionClosed = connectionClosed;

			return newContext;
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
