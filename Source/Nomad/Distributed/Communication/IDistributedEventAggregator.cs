using System.ServiceModel;
using Nomad.Messages;
using Nomad.Messages.Distributed;

namespace Nomad.Distributed.Communication
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDistributedEventAggregator" in both code and config file together.
	[ServiceContract]
	public interface IDistributedEventAggregator
	{
		[OperationContract]
		void OnPublish(NomadMessage message);
	}
}
