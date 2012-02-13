using System;

namespace Nomad.Communication.EventAggregation
{
	/// <summary>
	/// Provides means for publishing events, where listeners know little or nothing about publishers. Events are dispatched based on event type, not based on origin of the event
	/// </summary>
	public interface IEventAggregator
	{
		///<summary>
		///		Subscribes for events of specific type
		///</summary>
		/// <remarks>
		///		Assumes delivery in any thread
		/// </remarks>
		///<param name="action">Action to invoke when specific event is sent</param>
		///<typeparam name="T">type of message we want to listen</typeparam>
		/// <returns>ticket of subscription. Needed for unsubscription</returns>
		IEventAggregatorTicket<T> Subscribe<T>(Action<T> action) where T : class;

		
		///<summary>
		///		Subscribes for events of specific type, which should be delivered in specific way
		/// </summary>
		///<param name="action">Action to invoke when specific type of event is sent</param>
		///<param name="deliveryMethod">Way to deliver the message</param>
		///<typeparam name="T">type of message we want to listens</typeparam>
		///<returns>ticket of subscription. Needed for unsubscription</returns>
		IEventAggregatorTicket<T> Subscribe<T>(Action<T> action, DeliveryMethod deliveryMethod)
			where T : class;


		/// <summary>
		///		Notifies all subscribed members about passed <paramref name="message"/>
		/// </summary>
		/// <typeparam name="T">Type of message to send</typeparam>
		/// <param name="message">Message to send</param>
		/// <returns>True if called <see cref="IEventAggregator"/> implementing instance had subscriptions for <see cref="T"/></returns>
		bool Publish<T>(T message) where T : class;

		/// <summary>
		///		Notifies all subscribed members about passed <paramref name="message"/>.
		/// It also buffers the <paramref name="message"/> so it will be delivered to all future subscribers to this topic.
		/// </summary>
		/// <typeparam name="T">Type of message to send</typeparam>
		/// <param name="message">Message to send</param>
		/// <param name="validUntil">Time at which the message will expire and won't be deliverable any more.</param>
		/// <returns>True if called <see cref="IEventAggregator"/> implementing instance had subscriptions for <see cref="T"/></returns>
		void PublishTimelyBuffered<T>(T message, DateTime validUntil) where T : class;

		/// <summary>
		///		Notifies single subscribed member about passed <paramref name="message"/>
		/// Delivery semantic is chosen in <paramref name="singleDeliverySemantic"/> parameter.
		/// </summary>
		/// <remarks>
		///		When local delivering is used the <paramref name="singleDeliverySemantic"/> <c>is not used</c>. Any error in comunication
		/// wich would require delivery semantic in cross domain world of code sharing would mean serious problems. Thus giving attention
		/// to such thing in local computing would be overkill.
		/// </remarks>
		/// <typeparam name="T">Type of message to send</typeparam>
		/// <param name="message">Message to send</param>
		/// <param name="singleDeliverySemantic"><see cref="SingleDeliverySemantic"/> chosen for delivery </param>
		/// <returns>True if called <see cref="IEventAggregator"/> implementing instance had subscriptions for <see cref="T"/></returns>
		bool PublishSingleDelivery<T>(T message, SingleDeliverySemantic singleDeliverySemantic) where T : class;
	}
}