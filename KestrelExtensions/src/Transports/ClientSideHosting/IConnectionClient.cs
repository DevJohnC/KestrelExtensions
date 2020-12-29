using Microsoft.AspNetCore.Connections;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelExtensions.Transports.ClientSideHosting
{
	public interface IConnectionClient
	{
		ServerEndPoint ServerEndPoint { get; }

		Task<ConnectionContext> ConnectToServer(CancellationToken cancellationToken = default);
	}
}
