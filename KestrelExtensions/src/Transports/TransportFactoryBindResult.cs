using Microsoft.AspNetCore.Connections;
using System.Net;

namespace KestrelExtensions.Transports
{
	public struct TransportFactoryBindResult
	{
		public TransportFactoryBindResult(EndPoint endpoint) :
			this(false, endpoint, default)
		{
		}

		public TransportFactoryBindResult(bool didBind, EndPoint endpoint, IConnectionListener? connectionListener)
		{
			DidBind = didBind;
			EndPoint = endpoint;
			ConnectionListener = connectionListener;
		}

		public bool DidBind { get; }
		public EndPoint EndPoint { get; }
		public IConnectionListener? ConnectionListener { get; }
	}
}
