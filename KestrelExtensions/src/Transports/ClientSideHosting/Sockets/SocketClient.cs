using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting.Sockets
{
	internal class SocketClient : IConnectionClient
	{
		private readonly ILoggerFactory _loggerFactory;

		public SocketClient(EndPoint specifiedEndPoint, ILoggerFactory loggerFactory)
		{
			SpecifiedEndPoint = specifiedEndPoint;
			_loggerFactory = loggerFactory;
		}

		public EndPoint SpecifiedEndPoint { get; }

		public async Task<ConnectionContext> ConnectToServer(CancellationToken cancellationToken = default)
		{
			var endpoint = SpecifiedEndPoint;
			Socket socket;
			switch (endpoint)
			{
				case FileHandleEndPoint fileHandle:
					//  todo: preserve so that socketHandle can be disposed when socket use has completed
					var socketHandle = new SafeSocketHandle((IntPtr)fileHandle.FileHandle, ownsHandle: true);
					socket = new Socket(socketHandle);
					break;
				case UnixDomainSocketEndPoint unix:
					socket = new Socket(unix.AddressFamily, SocketType.Stream, ProtocolType.Unspecified);
					await Connect();
					break;
				case IPEndPoint ip:
					socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

					// Kestrel expects IPv6Any to bind to both IPv6 and IPv4
					if (ip.Address == IPAddress.IPv6Any)
					{
						socket.DualMode = true;
					}

					await Connect();
					break;
				default:
					socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					await Connect();
					break;
			}

			return SocketTransportFactory.CreateSocketConnection(socket, MemoryPool<byte>.Shared,
				PipeScheduler.ThreadPool, _loggerFactory);

			async ValueTask Connect()
			{
				await socket.ConnectAsync(endpoint, cancellationToken);
				if (socket.ProtocolType == ProtocolType.Tcp)
				{
					socket.NoDelay = true;
				}
			}
		}
	}
}
