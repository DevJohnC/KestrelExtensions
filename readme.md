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