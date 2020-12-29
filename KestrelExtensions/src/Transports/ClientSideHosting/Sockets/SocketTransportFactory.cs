using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting.Sockets
{
	static class SocketTransportFactory
	{
		private static object GetSocketsTrace(ILoggerFactory loggerFactory)
		{
			var logger = loggerFactory.CreateLogger("Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets");
			var assembly = typeof(Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketTransportFactory).Assembly;
			var types = assembly.GetTypes();
			var type = types.First(q => q.Name == "SocketsTrace");
			return Activator.CreateInstance(type, new object[] { logger });
		}

		public static ConnectionContext CreateSocketConnection(Socket socket,
								MemoryPool<byte> memoryPool,
								PipeScheduler transportScheduler,
								ILoggerFactory loggerFactory,
								long? maxReadBufferSize = null,
								long? maxWriteBufferSize = null,
								bool waitForData = true,
								bool useInlineSchedulers = false)
		{
			var socketsTrace = GetSocketsTrace(loggerFactory);
			var assembly = typeof(Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketTransportFactory).Assembly;
			var types = assembly.GetTypes();
			var type = types.First(q => q.Name == "SocketConnection");
			var ctor = type
				.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).First();

			var socketConnection = ctor.Invoke(new object[] {
				socket, memoryPool, transportScheduler, socketsTrace,
				maxReadBufferSize, maxWriteBufferSize, waitForData, useInlineSchedulers
			}) as ConnectionContext;

			type.GetMethod("Start").Invoke(socketConnection, null);

			return socketConnection;
		}
	}
}
