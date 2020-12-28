using Microsoft.AspNetCore.Connections;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public interface IConnectionClient
	{
		EndPoint SpecifiedEndPoint { get; }

		Task<ConnectionContext> ConnectToServer(CancellationToken cancellationToken = default);
	}
}
