using System;
using System.Collections.Generic;
using Nomad.Communication.EventAggregation;

namespace Nomad.Distributed.Communication.Deliveries.SingleDelivery
{
	/// <summary>
	///		The simplest possible implementation of <see cref="ISingleDeliverySubsystem"/>. The messages
	/// are volatile (if no subscribers are avaliable the message is lost). 
	/// 
	/// <para> 
	/// Algorithm used to find out the node to which the message is about to be sent is the <c>Tyran Algorithm</c>
	/// based on FIFO responses. 
	/// </para>
	/// </summary>
	public class BasicSingleDeliverySubsystem : ISingleDeliverySubsystem
	{
		#region ISingleDeliverySubsystem Members

		public bool SentSingle(IEnumerable<IDistributedEventAggregator> eventAggregators, byte[] messageContent,
		                       TypeDescriptor descriptor)
		{
			throw new NotImplementedException();
		}

		public bool RecieveSingle(IEventAggregator eventAggregator, object sendObject, Type type)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}