using System;
using System.Runtime.Serialization;

namespace KestrelExtensions.Transports
{
	[Serializable]
	public class TransportBindException : Exception
	{
		public TransportBindException()
		{
		}

		public TransportBindException(string? message) : base(message)
		{
		}

		public TransportBindException(string? message, Exception? innerException) : base(message, innerException)
		{
		}

		protected TransportBindException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}