using System.Net;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public interface IConnectionClient
	{
		EndPoint SpecifiedEndPoint { get; }
	}
}
