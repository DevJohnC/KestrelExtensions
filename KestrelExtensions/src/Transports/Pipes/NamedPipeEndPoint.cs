using System.Net;

namespace KestrelExtensions.Transports.Pipes
{
	public class NamedPipeEndPoint : IPEndPoint
	{
		public string PipeName { get; }

		public NamedPipeEndPoint(string pipeName) :
			base(0, 0)
		{
			PipeName = pipeName;
		}

		public override string ToString()
		{
			return $"net.pipe:{PipeName}";
		}
	}
}
