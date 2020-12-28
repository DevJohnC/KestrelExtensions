//  this is basically a copy of the SocketConnection transport written by Microsoft
//  https://github.com/dotnet/aspnetcore/blob/eaa6f080ba1dbb287be869ee0fc83918f7b26316/src/Servers/Kestrel/Transport.Sockets/src/Internal/SocketConnection.cs

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.Pipes
{
	public class PipeTransport : ConnectionContext
	{
		private readonly PipeStream _stream;
		private readonly ILogger<PipeTransport> _logger;
		private readonly IDuplexPipe _application;

		private readonly CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();
		private readonly object _shutdownLock = new object();
		private volatile bool _socketDisposed;
		private volatile Exception? _shutdownReason;
		private Task? _processingTask;

		private readonly TaskCompletionSource _waitForConnectionClosedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		private bool _connectionClosed;

		public PipeTransport(PipeStream pipeStream, ILogger<PipeTransport> logger)
		{
			ConnectionClosed = _connectionClosedTokenSource.Token;

			_stream = pipeStream;
			_logger = logger;

			var pipeOptions = new System.IO.Pipelines.PipeOptions(
				readerScheduler: PipeScheduler.ThreadPool,
				writerScheduler: PipeScheduler.ThreadPool,
				useSynchronizationContext: false
				);

			//  these pipes operate as pairs
			//  Transport is the pipes Kestrel will use to send and receive data
			//  Application is how this class will read data to send and buffer data that's read from the remote
			var pipes = DuplexPipe.CreateConnectionPair(pipeOptions, pipeOptions);
			Transport = pipes.Transport;
			_application = pipes.Application;
		}

		public override IDuplexPipe Transport { get; set; }

		public override string ConnectionId { get; set; } = Guid.NewGuid().ToString();

		public override IFeatureCollection Features { get; } = new FeatureCollection();

		public override IDictionary<object, object?> Items { get; set; } = new ConnectionItems();

		public void Start()
		{
			_processingTask = Run();
		}

		private async Task Run()
		{
			try
			{
				var recvTask = DoRecv();
				var sendTask = DoSend();

				await recvTask;
				await sendTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error in PipeTransport.Run.");
			}
		}

		private async Task DoRecv()
		{
			Exception? error = null;
			var input = _application.Output;

			try
			{
				while (true)
				{
					var buffer = input.GetMemory(4096);
					var bytesReceived = await _stream.ReadAsync(buffer);

					if (bytesReceived == 0)
					{
						//  stream closed
						break;
					}

					input.Advance(bytesReceived);

					var result = await input.FlushAsync();

					if (result.IsCompleted || result.IsCanceled)
					{
						// Pipe consumer is shut down, do we stop writing
						break;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Pipe receive error");
				error = ex;
			}
			finally
			{
				input.Complete(error);
				FireConnectionClosed();
				await _waitForConnectionClosedTcs.Task;
			}
		}

		private async Task DoSend()
		{
			Exception? error = null;
			var output = _application.Input;

			try
			{
				while (true)
				{
					var result = await output.ReadAsync();

					if (result.IsCanceled)
					{
						break;
					}

					var buffer = result.Buffer;

					var end = buffer.End;
					var isCompleted = result.IsCompleted;
					if (!buffer.IsEmpty)
					{
						foreach (var segment in buffer)
						{
							await _stream.WriteAsync(segment);
						}
					}

					output.AdvanceTo(end);

					if (isCompleted)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Pipe send error");
				error = ex;
			}
			finally
			{
				Shutdown(error);
				output.Complete(error);
				_application.Output.CancelPendingFlush();
			}
		}

		private void FireConnectionClosed()
		{
			// Guard against scheduling this multiple times
			if (_connectionClosed)
			{
				return;
			}

			_connectionClosed = true;

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				state.CancelConnectionClosedToken();

				state._waitForConnectionClosedTcs.TrySetResult();
			},
			this,
			preferLocal: false);
		}

		public override void Abort(ConnectionAbortedException abortReason)
		{
			// Try to gracefully close the pipe.
			Shutdown(abortReason);

			// Cancel ProcessSends loop after calling shutdown to ensure the correct _shutdownReason gets set.
			_application.Input.CancelPendingRead();
		}

		private void Shutdown(Exception? shutdownReason)
		{
			lock (_shutdownLock)
			{
				if (_socketDisposed)
				{
					return;
				}

				// Make sure to close the connection only after the _aborted flag is set.
				// Without this, the RequestsCanBeAbortedMidRead test will sometimes fail when
				// a BadHttpRequestException is thrown instead of a TaskCanceledException.
				_socketDisposed = true;

				// shutdownReason should only be null if the output was completed gracefully, so no one should ever
				// ever observe the nondescript ConnectionAbortedException except for connection middleware attempting
				// to half close the connection which is currently unsupported.
				_shutdownReason = shutdownReason ?? new ConnectionAbortedException("The pipe transport's send loop completed gracefully.");

				_logger.LogInformation("Shutdown connection {0}: {1}", ConnectionId, _shutdownReason.Message);

				try
				{
					_stream.Close();
				}
				catch
				{
					// Ignore any errors from _stream.Close() since we're tearing down the connection anyway.
				}

				_stream.Dispose();
			}
		}

		public override async ValueTask DisposeAsync()
		{
			Transport.Input.Complete();
			Transport.Output.Complete();

			if (_processingTask != null)
			{
				await _processingTask;
			}

			_connectionClosedTokenSource.Dispose();
		}

		private void CancelConnectionClosedToken()
		{
			try
			{
				_connectionClosedTokenSource.Cancel();
			}
			catch (Exception ex)
			{
				_logger.LogError(0, ex, $"Unexpected exception in {nameof(PipeTransport)}.{nameof(CancelConnectionClosedToken)}.");
			}
		}
	}
}
