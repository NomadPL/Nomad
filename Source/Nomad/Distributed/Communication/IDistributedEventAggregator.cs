using System;
using System.ServiceModel;
using Nomad.Communication.EventAggregation;
using Nomad.Distributed.Communication.Deliveries.SingleDelivery;
using Nomad.Distributed.Communication.Deliveries.TimedDelivery;
using Nomad.Distributed.Communication.Deliveries.TopicDelivery;
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
		void OnPublishControl(NomadDistributedMessage message);

		/// <summary>
		///		Invoked by other <see cref="DistributedEventAggregator"/> instances
		/// to send <c>user defined messages</c>.
		/// </summary>
		/// <remarks>
		///		Called by <see cref="ITopicDeliverySubsystem"/>.
		/// </remarks>
		[OperationContract]
		bool OnPublish(byte[] byteStream, TypeDescriptor typeDescriptor);

		/// <summary>
		///		Invoked by other <see cref="DistributedEventAggregator"/> especially <see cref="ISingleDeliverySubsystem"/>
		/// for implementing <see cref="IEventAggregator.PublishSingleDelivery{T}"/>.
		/// </summary>
		/// <param name="byteStream"></param>
		/// <param name="typeDescriptor"></param>
		/// <returns>If deliver was successful (was somebody waiting for this message) then mark it true. Otherwise false.</returns>
		[OperationContract]
		bool OnPublishSingleDelivery(byte[] byteStream, TypeDescriptor typeDescriptor);

		/// <summary>
		///		Invoked by other <see cref="DistributedEventAggregator"/> instances (especially <see cref="ITimedDeliverySubsystem"/>
		/// to send some user defined message with <see cref="IEventAggregator.PublishTimelyBuffered{T}"/>
		/// </summary>		
		[OperationContract]
		void OnPublishTimelyBufferedDelivery(byte[] byteStream, TypeDescriptor typeDescriptor,DateTime voidTime);

		/// <summary>
		///		Control message used to check if current <see cref="IDistributedEventAggregator"/> has in its local
		/// <see cref="IEventAggregator"/> any subscribers for this kind of message, denoted by <paramref name="descriptor"/>.
		/// </summary>
		/// <param name="descriptor">The type of the message to be sent</param>
		/// <returns>True if there is subscriber for message of type <paramref name="descriptor"/></returns>
		[OperationContract]
		bool IsSubscriberForType(TypeDescriptor descriptor);
	}
}