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
		private bool _unbound;

		public ServerConnectionManager(ILogger<ServerConnectionManager> logger, IConnectionClient client)
		{
			_logger = logger;
			_client = client;
		}

		public EndPoint EndPoint => _client.ServerEndPoint;

		public async ValueTask<ConnectionContext> AcceptAsync(CancellationToken cancellationToken = default)
		{
			if (_currentConnection != null && _connectionClosed != null)
			{
				await _connectionClosed.Task;
			}

			if (_unbound)
				return null;

			while (!cancellationToken.IsCancellationRequested &&
				!_unbound)
			{
				try
				{
					var newContext = await _client.ConnectToServer(cancellationToken);
					var connectionClosed = new TaskCompletionSource();

					newContext.ConnectionClosed.Register(() =>
					{
						_logger.LogInformation("Server connection to {0} lost", _client.ServerEndPoint);
						connectionClosed.SetResult();
					});

					_currentConnection = newContext;
					_connectionClosed = connectionClosed;

					_logger.LogInformation("Server connection to {0} established", _client.ServerEndPoint);

					return newContext;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error establishing connection to {0}", _client.ServerEndPoint);
				}
			}

			return null;
		}

		public ValueTask DisposeAsync()
		{
			return default;
		}

		public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
		{
			_unbound = true;
			_currentConnection?.Abort();
			return default;
		}
	}
}
