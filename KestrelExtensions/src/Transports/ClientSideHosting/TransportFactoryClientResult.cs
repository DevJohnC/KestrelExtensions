using System.Net;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public struct TransportFactoryClientResult
	{
		public TransportFactoryClientResult(EndPoint endpoint) :
			this(false, endpoint, default)
		{
		}

		public TransportFactoryClientResult(bool didCreateClient, EndPoint endpoint, IConnectionClient? connectionClient)
		{
			DidCreateClient = didCreateClient;
			EndPoint = endpoint;
			ConnectionClient = connectionClient;
		}

		public bool DidCreateClient { get; }
		public EndPoint EndPoint { get; }
		public IConnectionClient? ConnectionClient { get; }
	}
}
