using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.Pipes
{
	internal class NamedPipeServer : IAsyncDisposable
	{
		private readonly List<NamedPipeServerStream> _servers = new List<NamedPipeServerStream>();
		private readonly List<Task<(bool Success, NamedPipeServerStream PipeServer)>> _acceptTasks = new List<Task<(bool, NamedPipeServerStream)>>();
		private int _serverCount = -1;
		private int _maxServerCount = -1;

		public NamedPipeServer(NamedPipeEndPoint endpoint)
		{
			EndPoint = endpoint;
		}

		public NamedPipeEndPoint EndPoint { get; }

		public async ValueTask DisposeAsync()
		{
			foreach (var server in _servers)
			{
				await server.DisposeAsync();
			}
			_servers.Clear();
		}

		public void Listen(int serverCount = 4, int maxServerCount = -1)
		{
			if (_serverCount > 0)
			{
				throw new InvalidOperationException("Server is already listening.");
			}

			if (maxServerCount == -1)
			{
				maxServerCount = NamedPipeServerStream.MaxAllowedServerInstances;
			}

			_serverCount = serverCount;
			_maxServerCount = maxServerCount;

			for (var i = 0; i < _serverCount; i++)
			{
				var pipeServer = CreateServerStream();
				_servers.Add(pipeServer);
				_acceptTasks.Add(TryWaitForConnection(pipeServer));
			}
		}

		public async Task<NamedPipeServerStream?> Accept(CancellationToken cancellationToken)
		{
			var cancellationTask = WaitForCancellation(cancellationToken);
			var acceptTasks = new List<Task>();
			acceptTasks.Add(cancellationTask);
			acceptTasks.AddRange(_acceptTasks);
			while (!cancellationToken.IsCancellationRequested)
			{
				var completedTask = await Task.WhenAny(acceptTasks);
				acceptTasks.Remove(completedTask);

				if (completedTask is Task<(bool Success, NamedPipeServerStream PipeServer)> acceptedTask)
				{
					var acceptedStream = await acceptedTask;
					_acceptTasks.Remove(acceptedTask);
					_servers.Remove(acceptedStream.PipeServer);

					var newStream = CreateServerStream();
					var waitTask = TryWaitForConnection(newStream);
					acceptTasks.Add(waitTask);
					_acceptTasks.Add(waitTask);
					_servers.Add(newStream);

					if (acceptedStream.Success)
						return acceptedStream.PipeServer;

					//  todo: continue to attempt to accept connections if the server isn't being disposed
					return null;
				}
			}

			return null;

			async Task WaitForCancellation(CancellationToken cancellationToken)
			{
				while (!cancellationToken.IsCancellationRequested)
					await Task.Delay(TimeSpan.FromSeconds(0.1));
			}
		}

		private async Task<(bool, NamedPipeServerStream)> TryWaitForConnection(NamedPipeServerStream pipeServer)
		{
			try
			{
				await pipeServer.WaitForConnectionAsync();
				return (true, pipeServer);
			}
			catch
			{
				//  todo: log exception
				return (false, pipeServer);
			}
		}

		private NamedPipeServerStream CreateServerStream()
		{
			return new NamedPipeServerStream(EndPoint.PipeName, PipeDirection.InOut, _maxServerCount,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
		}
	}
}
