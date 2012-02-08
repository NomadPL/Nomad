using System.ServiceModel;
using Nomad.Messages;
using Nomad.Messages.Distributed;

namespace Nomad.Distributed.Communication
{
	[ServiceContract]
	public interface IDistributedEventAggregator
	{
		/// <summary>
		///		Invoked by other  <see cref="DistributedEventAggregator"/> instances 
		/// to send <c>control</c> messages for internal <see cref="Nomad"/> use.
		/// </summary>
		/// <param name="message"></param>
		[OperationContract]
		void OnPublishControl(NomadMessage message);

		/// <summary>
		///		Invoked by other <see cref="DistributedEventAggregator"/> instances
		/// to send <c>user defined messages</c>.
		/// </summary>
		[OperationContract]
		void OnPublish(byte[] byteStream, TypeDescriptor typeDescriptor);
	}
}