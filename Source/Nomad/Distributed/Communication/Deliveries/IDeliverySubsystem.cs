using System.Collections.Generic;
using Nomad.Messages.Distributed;

namespace Nomad.Distributed.Communication.Deliveries
{
	/// <summary>
	///		Describes the typical sending mechanism.
	/// </summary>
	public interface IDeliverySubsystem
	{
		/// <summary>
		///		Sends the control message to collection of possible <see cref="IDistributedEventAggregator"/>. 
		/// </summary>
		/// <param name="eventAggregators">The collection of other counterparts of the system avaliable to this subsystem</param>
		/// <param name="msg">Control message to be sent</param>
		/// <returns>True if all the sending succeeded</returns>
		bool SendControl(IEnumerable<IDistributedEventAggregator> eventAggregators, NomadDistributedMessage msg);
	}
}