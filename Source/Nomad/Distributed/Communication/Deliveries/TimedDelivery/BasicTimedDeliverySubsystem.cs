using System;
using System.Collections.Generic;
using System.Reflection;
using Nomad.Communication.EventAggregation;
using Nomad.Core;
using Nomad.Distributed.Communication.Utils;
using Nomad.Messages.Loading;

namespace Nomad.Distributed.Communication.Deliveries.TimedDelivery
{
	/// <summary>
	/// This class implements the basic binaryMessages buffer for messages that might be later deserialized.
	/// This feature occurs when new <see cref="Assembly"/> are loaded into the <see cref="AppDomain"/>.
	/// </summary>
	[Serializable]
	public class BasicTimedDeliverySubsystem : ITimedDeliverySubsystem
	{
		private readonly List<BufferedBinaryMessage> _bufferedBinaryMessages = new List<BufferedBinaryMessage>();


		public void AddMessageToBuffer(BufferedBinaryMessage message)
		{
			_bufferedBinaryMessages.Add(message);
		}

		public void TryDeliverBufferedMessages(IEventAggregator localEA)
		{
			var deliveredMessages = new List<BufferedBinaryMessage>();

			foreach (var bufferedBinaryMessage in _bufferedBinaryMessages)
			{
				// omit already expired messages
				if (bufferedBinaryMessage.ExpiryTime <= DateTime.Now) continue;

				// try deserialize
				object message;
				Type type;
				try
				{
					MessageSerializer.UnPackData(bufferedBinaryMessage.Descriptor, bufferedBinaryMessage.Message, out message, out type);
				}
				catch
				{
					// binary message is yet not deliverable
					continue;
				}

				// message is deliverable
				// invoke this generic method with type T
				MethodInfo methodInfo = localEA.GetType().GetMethod("PublishTimelyBuffered");
				MethodInfo goodMethodInfo = methodInfo.MakeGenericMethod(type);
				goodMethodInfo.Invoke(localEA, new[] {message, bufferedBinaryMessage.ExpiryTime});

				// add to delivered
				deliveredMessages.Add(bufferedBinaryMessage);
			}

			// clean the buffer from delivered messages
			foreach (var deliveredMessage in deliveredMessages)
			{
				_bufferedBinaryMessages.Remove(deliveredMessage);
			}

			// clean binary buffer from expired messages
			_bufferedBinaryMessages.RemoveAll(m => m.ExpiryTime <= DateTime.Now);
		}
	}
}