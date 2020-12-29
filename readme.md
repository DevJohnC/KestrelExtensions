# KestrelExtenions

![.NET](https://github.com/DevJohnC/KestrelExtensions/workflows/.NET/badge.svg) [![MIT License](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/DevJohnC/KestrelExtensions/blob/master/license.txt)

KestrelExtensions is a library for adding extra functionality to ASP.NET Core's default server, Kestrel.

## Named Pipes

Named pipes give you the ability to implement IPC using Kestrel, perhaps utilizing gRPC, without the need to go through network stack.

To enable named pipes add it to your ASP.NET Core or Kestrel project like so:

```
private static IHostBuilder CreateHostBuilder() =>
	Host.CreateDefaultBuilder()
		.ConfigureServices(services => services.UsePipesTransport())
		.ConfigureWebHostDefaults(builder =>
		{
			builder.ConfigureKestrel(options =>
			{
				options.Listen(new NamedPipeEndPoint("myIpcPipe"));
			});
			builder.UseStartup<Startup>();
		});
```

It may be helpful to know how to configure an HttpClient to connect to Kestrel over named pipes:

```
var socketsHandler = new SocketsHttpHandler
{
	ConnectCallback = async (ctx, cancellationToken) =>
	{
		var stream = new NamedPipeClientStream(".", "myIpcPipe", PipeDirection.InOut,
			PipeOptions.Asynchronous);
		await stream.ConnectAsync();
		return stream;
	}
};
var httpClient = new HttpClient(socketsHandler);
```

## Outbound Server-to-Client Connections

The Server-to-Client transport is intended for use cases where you need to connect hosted services to a client from the server.

Admittedly this sounds a bit confusing. Think of an application installed on a customers workstation and you need to call gRPC services
from an internet server. Having the workstation connect to the internet service and exposing gRPC services over that established
connection removes the headaches of trying to host a server behind firewalls and NATs.

Example:

```
private static IHostBuilder CreateHostBuilder() =>
	Host.CreateDefaultBuilder()
		.ConfigureServices(services => services.UseServerToClientTransport())
		.ConfigureWebHostDefaults(builder =>
		{
			builder.ConfigureKestrel(options =>
			{
				options.Listen(new ServerEndPoint(
					new IPEndPoint(
						IPAddress.Parse("x.x.x.x"), 9999
					)
				));
			});
			builder.UseStartup<Startup>();
		});
```