using System;
using System.Collections.Generic;

namespace Nomad.Distributed.Communication.Deliveries.TimedDelivery
{
	[Serializable]
	public class BasicTimedDeliverySubsystem : ITimedDeliverySubsystem
	{
		public bool SendTimed(IEnumerable<IDistributedEventAggregator> eventAggregators)
		{
			throw new NotImplementedException();
		}
	}
}