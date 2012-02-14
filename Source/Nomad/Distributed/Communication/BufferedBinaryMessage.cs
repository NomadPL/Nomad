using System;

namespace Nomad.Distributed.Communication
{
	/// <summary>
	/// Class used to hold messages that need to be buffered and possibly delivered in the future.
	/// </summary>
	internal class BufferedBinaryMessage
	{
		public TypeDescriptor Descriptor { get; private set; }
		public byte[] Message { get; private set; }
		public DateTime ExpiryTime { get; private set; }

		public BufferedBinaryMessage(byte[] message, DateTime expiryTime, TypeDescriptor descriptor)
		{
			Message = message;
			ExpiryTime = expiryTime;
			Descriptor = descriptor;
		}
	}
}