using System;

namespace Nomad.Communication.EventAggregation
{
	/// <summary>
	/// Class used to hold messages that need to be buffered and possibly delivered in the future.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class BufferedMessage
	{
		public object Message { get; private set; }
		public DateTime ExpiryTime { get; private set; }

		public BufferedMessage(object message, DateTime expiryTime)
		{
			Message = message;
			ExpiryTime = expiryTime;
		}
	}
}