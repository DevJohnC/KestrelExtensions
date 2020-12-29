using KestrelExtensions.Transports.Pipes;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting.Pipes
{
	internal class PipeClient : IConnectionClient
	{
		private readonly NamedPipeEndPoint _endpoint;


		public PipeClient(ServerEndPoint endpoint, ILoggerFactory loggerFactory)
		{
			_endpoint = (NamedPipeEndPoint)endpoint.EndPoint;
			ServerEndPoint = endpoint;
			_loggerFactory = loggerFactory;
		}

		public ServerEndPoint ServerEndPoint { get; }

		private readonly ILoggerFactory _loggerFactory;

		public async Task<ConnectionContext> ConnectToServer(CancellationToken cancellationToken = default)
		{
			var pipeClient = new NamedPipeClientStream(".", _endpoint.PipeName, PipeDirection.InOut,
				PipeOptions.Asynchronous);
			await pipeClient.ConnectAsync(cancellationToken);
			var transport = new PipeTransport(pipeClient, _loggerFactory.CreateLogger<PipeTransport>());
			transport.Start();
			return transport;
		}
	}
}
