using System.Collections.Generic;
using Nomad.Communication.EventAggregation;

namespace Nomad.Distributed.Communication.Deliveries.TimedDelivery
{
	/// <summary>
	/// Interface used to implement temporarily notDeserializable messages that need to be delivered in the future.
	/// </summary>
	public interface ITimedDeliverySubsystem
	{
		/// <summary>
		/// Adds unserializable message to deliver later buffer.
		/// </summary>
		/// <param name="message"></param>
		void AddMessageToBuffer(BufferedBinaryMessage message);

		/// <summary>
		/// Tries to deliver buffered binary messages.
		/// </summary>
		/// <param name="localEA">Local EventAggregator</param>
		void TryDeliverBufferedMessages(IEventAggregator localEA);
	}
}