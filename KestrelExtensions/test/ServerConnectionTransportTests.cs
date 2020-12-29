using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Xunit;
using System.Net.Http;
using System.IO;
using KestrelExtensions.Transports.ClientSideHosting;
using System.Net;
using System.Net.Sockets;
using KestrelExtensions.Transports.Pipes;
using System.IO.Pipes;

namespace KestrelExtensions.Tests
{
	public class ServerConnectionTransportTests
	{
		[Fact]
		public async Task Can_Make_Http_Requests_Over_Server_Socket()
		{
			var ipEndPoint = new IPEndPoint(IPAddress.Loopback, 61245);
			var tcpServer = new TcpListener(ipEndPoint);
			tcpServer.Start(1);

			var webHost = CreateHostBuilder(ipEndPoint).Build();
			await webHost.StartAsync();

			using var tcpClient = await tcpServer.AcceptTcpClientAsync();
			var socketsHandler = new SocketsHttpHandler
			{
				ConnectCallback = (ctx, cancelToken) => ValueTask.FromResult<Stream>(tcpClient.GetStream())
			};
			var httpClient = new HttpClient(socketsHandler);
			var response = await httpClient.GetStringAsync("http://localhost/");

			Assert.Equal("Hello World!", response);

			tcpServer.Stop();

			await webHost.StopAsync();
		}

		[Fact]
		public async Task Can_Make_Http_Requests_Over_Server_NamedPipe()
		{
			using var namedPipeServer = new NamedPipeServerStream("http_pipe", PipeDirection.InOut,
				1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

			var webHost = CreateHostBuilder(new NamedPipeEndPoint("http_pipe")).Build();
			await webHost.StartAsync();

			await namedPipeServer.WaitForConnectionAsync();
			var socketsHandler = new SocketsHttpHandler
			{
				ConnectCallback = (ctx, cancelToken) => ValueTask.FromResult<Stream>(namedPipeServer)
			};
			var httpClient = new HttpClient(socketsHandler);
			var response = await httpClient.GetStringAsync("http://localhost/");

			Assert.Equal("Hello World!", response);

			await webHost.StopAsync();
		}

		private static IHostBuilder CreateHostBuilder(EndPoint serverEndPoint) =>
			Host.CreateDefaultBuilder()
				.ConfigureServices(services => services.UseServerTransport())
				.ConfigureWebHostDefaults(builder =>
				{
					builder.ConfigureKestrel(options =>
					{
						options.Listen(new ServerEndPoint(serverEndPoint));
					});
					builder.UseStartup<HttpEndpointStartup>();
				});

		private class HttpEndpointStartup
		{
			public void Configure(IApplicationBuilder app)
			{
				app.UseRouting();

				app.UseEndpoints(endpoints =>
				{
					endpoints.MapGet("/", async context =>
					{
						await context.Response.WriteAsync("Hello World!");
					});
				});
			}
		}
	}
}
