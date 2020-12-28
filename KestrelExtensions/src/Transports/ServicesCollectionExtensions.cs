using KestrelExtensions.Transports;
using KestrelExtensions.Transports.ClientSideHosting;
using KestrelExtensions.Transports.Pipes;
using KestrelExtensions.Transports.Sockets;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServicesCollectionExtensions
	{
		/// <summary>
		/// Add support for an endpoint transport.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection UseTransport<T>(this IServiceCollection services)
			where T : class, ITrialConnectionListenerFactory
		{
			services.AddSingleton<IConnectionListenerFactory, CompositeTransportFactory>();
			services.AddSingleton<ITrialConnectionListenerFactory, T>();
			return services;
		}

		/// <summary>
		/// Add support for pipe endpoints.
		/// </summary>
		/// <remarks>This will remove the default sockets support. To keep using sockets add support with UseSockets().</remarks>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection UsePipesTransport(this IServiceCollection services)
		{
			services.UseTransport<TrialPipesTransportFactory>();
			return services;
		}

		/// <summary>
		/// Add support for socket endpoints (IPEndPoint, UnixEndPoint and FileHandleEndPoint).
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection UseSocketsTransport(this IServiceCollection services)
		{
			services.AddSingleton<SocketTransportFactory>();
			services.UseTransport<TrialSocketTransportFactory>();
			return services;
		}

		/// <summary>
		/// Add support for hosting Kestrel on outbound connections established to a server from this client.
		/// </summary>
		/// <remarks>This will remove the default sockets support. To keep using sockets add support with UseSockets().</remarks>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection UseServerTransport(this IServiceCollection services)
		{
			services.UseTransport<TrialServerTransportFactory>();
			return services;
		}
	}
}
