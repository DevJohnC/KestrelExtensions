using Microsoft.AspNetCore.Connections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports
{
	/// <summary>
	/// A more refined factory for creating <see cref="IConnectionListener" /> instances.
	/// 
	/// Utilizes <see cref="ITrialConnectionListenerFactory"/> instances to attempt to create
	/// an <see cref="IConnectionListener"/> that is capable of binding to different endpoints.
	/// </summary>
	public class CompositeTransportFactory : IConnectionListenerFactory
	{
		private readonly List<ITrialConnectionListenerFactory> _factories;

		public CompositeTransportFactory(IEnumerable<ITrialConnectionListenerFactory> factories)
		{
			_factories = factories.ToList();
		}

		public async ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
		{
			foreach (var factory in _factories)
			{
				var result = await factory.TryBindAsync(endpoint, cancellationToken);
				if (result.DidBind &&
					result.ConnectionListener != null)
				{
					return result.ConnectionListener;
				}
			}

			throw new TransportBindException("No factory could bind to the provided endpoint.");
		}
	}
}
