using System.Collections.Generic;
using Nomad.Communication.EventAggregation;

namespace Nomad.Distributed.Communication.Deliveries.TimedDelivery
{
	/// <summary>
	///		The implementation of the delivering method
	/// for publish on the <see cref="IEventAggregator.PublishTimelyBuffered{T}"/> method.
	/// </summary>
	public interface ITimedDeliverySubsystem 
	{
		/// <summary>
		///		TODO: make this code work
		/// </summary>
		/// <param name="eventAggregators"></param>
		/// <returns></returns>
		bool SendTimed(IEnumerable<IDistributedEventAggregator> eventAggregators);
	}
}