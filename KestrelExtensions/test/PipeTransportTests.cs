using KestrelExtensions.Transports.Pipes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.IO.Pipes;
using Xunit;
using System.Net.Http;
using System.IO;
using System;

namespace KestrelExtensions.Tests
{
	public class PipeTransportTests
	{
		private static IHost _webHost = CreateHostBuilder().Build();

		static PipeTransportTests()
		{
			_webHost.Start();
		}

		[Fact]
		public async Task Can_Make_Http1_Requests_Over_Named_Pipe()
		{
			var socketsHandler = new SocketsHttpHandler
			{
				ConnectCallback = async (ctx, cancellationToken) => await CreatePipeStream("http_1_Pipe")
			};
			var httpClient = new HttpClient(socketsHandler)
			{
				DefaultRequestVersion = new Version(1, 1),
				DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
			};
			var response = await httpClient.GetStringAsync("http://localhost/");

			Assert.Equal("Hello World!", response);
		}

		[Fact]
		public async Task Can_Make_Http2_Requests_Over_Named_Pipe()
		{
			var socketsHandler = new SocketsHttpHandler
			{
				ConnectCallback = async (ctx, cancellationToken) => await CreatePipeStream("http_2_Pipe")
			};
			var httpClient = new HttpClient(socketsHandler)
			{
				DefaultRequestVersion = new Version(2, 0),
				DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
			};
			var response = await httpClient.GetStringAsync("http://localhost/");

			Assert.Equal("Hello World!", response);
		}

		/*[Fact]
		public async Task Can_Make_Http2_SSL_Requests_Over_Named_Pipe()
		{
			var socketsHandler = new SocketsHttpHandler
			{
				ConnectCallback = async (ctx, cancellationToken) => await CreatePipeStream("http_2_ssl_Pipe")
			};
			var httpClient = new HttpClient(socketsHandler)
			{
				DefaultRequestVersion = new Version(2, 0),
				DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
			};
			var response = await httpClient.GetStringAsync("https://localhost/");

			Assert.Equal("Hello World!", response);
		}*/

		private static async Task<NamedPipeClientStream> CreatePipeStream(string pipeName)
		{
			var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut,
				PipeOptions.Asynchronous);
			await stream.ConnectAsync();
			return stream;
		}

		private static IHostBuilder CreateHostBuilder() =>
			Host.CreateDefaultBuilder()
				.ConfigureServices(services => services.UsePipesTransport())
				.ConfigureWebHostDefaults(builder =>
				{
					builder.ConfigureKestrel(options =>
					{
						options.Listen(new NamedPipeEndPoint("http_1_Pipe"), endpointOpts =>
						{
							endpointOpts.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
						});
						options.Listen(new NamedPipeEndPoint("http_2_Pipe"), endpointOpts =>
						{
							endpointOpts.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
						});
						options.Listen(new NamedPipeEndPoint("http_2_ssl_Pipe"), endpointOpts =>
						{
							endpointOpts.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
							endpointOpts.UseHttps();
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
