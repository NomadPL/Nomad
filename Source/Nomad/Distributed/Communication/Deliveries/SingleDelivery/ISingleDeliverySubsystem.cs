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
		/// <param name="eventAggregators"></param>
		/// <returns></returns>
		bool SentSingle(IEnumerable<IDistributedEventAggregator> eventAggregators);
	}
}