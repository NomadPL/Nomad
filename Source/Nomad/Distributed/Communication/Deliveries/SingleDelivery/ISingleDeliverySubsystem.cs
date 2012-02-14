using System;
using System.Collections.Generic;
using Nomad.Communication.EventAggregation;

namespace Nomad.Distributed.Communication.Deliveries.SingleDelivery
{
	/// <summary>
	///		Describes various algorithms for obtaining the <see cref="IEventAggregator.PublishSingleDelivery{T}"/> 
	/// with the respect to <see cref="SingleDeliverySemantic"/> values.
	/// </summary>
	/// <remarks>
	///		Various implementation might provide different means (like speed up) of such implementation.
	/// </remarks>
	public interface ISingleDeliverySubsystem 
	{
		/// <summary>
		///		Sends the message as the typical single delivery.
		/// </summary>
		///<param name="eventAggregators"></param>
		///<param name="messageContent"></param>
		///<param name="descriptor"></param>
		///<param name="delivery"></param>
		///<returns></returns>
		bool SentSingle(IEnumerable<IDistributedEventAggregator> eventAggregators, byte[] messageContent, TypeDescriptor descriptor, SingleDeliverySemantic delivery);

		/// <summary>
		///		Revieves the message as does something with it.
		/// </summary>
		/// <param name="eventAggregator"></param>
		/// <param name="sendObject"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		bool RecieveSingle(IEventAggregator eventAggregator, object sendObject, Type type);
	}
}