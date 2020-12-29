using System.Net;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public class ServerEndPoint : IPEndPoint
	{
		public EndPoint EndPoint { get; }

		public ServerEndPoint(EndPoint endpoint) :
			base(0, 0)
		{
			EndPoint = endpoint;
		}

		public override string ToString()
		{
			return $"server:{EndPoint}";
		}
	}
}
