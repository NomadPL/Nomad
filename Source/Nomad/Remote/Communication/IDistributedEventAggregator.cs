using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Nomad.Messages;

namespace Nomad.Remote.Communication
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDistributedEventAggregator" in both code and config file together.
	[ServiceContract]
	public interface IDistributedEventAggregator
	{
		[OperationContract]
		void OnPublish(NomadMessage message);
	}
}
