using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Xunit;
using System.Net.Http;
using System.IO;
using System;
using KestrelExtensions.Transports.ClientSideHosting;
using System.Net;
using System.Net.Sockets;

namespace KestrelExtensions.Tests
{
	public class ServerConnectionTransportTests
	{
		[Fact]
		public async Task Can_Make_Http1_Requests_Over_Server_Socket()
		{
			var tcpServer = new TcpListener(IPAddress.Loopback, 61245);
			tcpServer.Start(1);

			var webHost = CreateHostBuilder().Build();
			await webHost.StartAsync();

			var tcpClient = await tcpServer.AcceptTcpClientAsync();
			var socketsHandler = new SocketsHttpHandler
			{
				ConnectCallback = (ctx, cancelToken) => ValueTask.FromResult<Stream>(tcpClient.GetStream())
			};
			var httpClient = new HttpClient(socketsHandler)
			{
				DefaultRequestVersion = new Version(1, 1),
				DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
			};
			var response = await httpClient.GetStringAsync("http://localhost/");

			Assert.Equal("Hello World!", response);
		}

		private static IHostBuilder CreateHostBuilder() =>
			Host.CreateDefaultBuilder()
				.ConfigureServices(services => services.UseServerTransport())
				.ConfigureWebHostDefaults(builder =>
				{
					builder.ConfigureKestrel(options =>
					{
						options.Listen(new ServerEndPoint(new IPEndPoint(IPAddress.Loopback, 61245)), endpointOpts =>
						{
							endpointOpts.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
						});
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
