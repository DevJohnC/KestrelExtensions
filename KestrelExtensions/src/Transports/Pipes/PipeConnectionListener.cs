using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.Pipes
{
	internal class PipeConnectionListener : IConnectionListener
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger<PipeConnectionListener> _logger;
		private readonly NamedPipeEndPoint _endpoint;
		private NamedPipeServer? _server;

		public EndPoint EndPoint => _endpoint;

		public PipeConnectionListener(ILoggerFactory loggerFactory, NamedPipeEndPoint endpoint)
		{
			_loggerFactory = loggerFactory;
			_logger = loggerFactory.CreateLogger<PipeConnectionListener>();
			_endpoint = endpoint;
		}

		internal void Bind()
		{
			if (_server != null)
			{
				throw new InvalidOperationException("Transport is already bound.");
			}

			var server = new NamedPipeServer(_endpoint);
			//  todo: configure server size and max server instances from an IOptions<T>
			server.Listen();

			_server = server;
		}

		public async ValueTask<ConnectionContext> AcceptAsync(CancellationToken cancellationToken = default)
		{
			if (_server == null)
			{
				throw new InvalidOperationException("Server not bound, call Bind() before attempting to accept connections.");
			}

			while (!cancellationToken.IsCancellationRequested)
			{
				var pipeServer = await _server.Accept(cancellationToken);
				if (pipeServer == null)
				{
					continue;
				}

				var transport = new PipeTransport(pipeServer, _loggerFactory.CreateLogger<PipeTransport>());
				transport.Start();
				return transport;
			}

			return null;
		}

		public ValueTask DisposeAsync()
		{
			return _server?.DisposeAsync() ?? default;
		}

		public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
		{
			return _server?.DisposeAsync() ?? default;
		}
	}
}
