using Microsoft.AspNetCore.Connections;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting.Sockets
{
	internal class SocketClient : IConnectionClient
	{
		public SocketClient(EndPoint specifiedEndPoint)
		{
			SpecifiedEndPoint = specifiedEndPoint;
		}

		public EndPoint SpecifiedEndPoint { get; }

		public Task<ConnectionContext> ConnectToServer(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
