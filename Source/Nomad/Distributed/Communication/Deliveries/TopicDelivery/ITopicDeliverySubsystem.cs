using System.Collections.Generic;

namespace Nomad.Distributed.Communication.Deliveries.TopicDelivery
{
	/// <summary>
	///		How the multiple delivery algorithms should behave.
	/// </summary>
	public interface ITopicDeliverySubsystem 
	{
		/// <summary>
		///		Sends the <paramref name="messageContent"/> to <c>all</c> subscribed <see cref="IDistributedEventAggregator"/> instances
		/// passed as <paramref name="eventAggregators"/>.
		/// </summary>
		/// <param name="eventAggregators">Where to sent</param>
		///<param name="messageContent">What  message</param>
		///<param name="descriptor">What kind of this is</param>
		///<returns></returns>
		bool SentAll(IEnumerable<IDistributedEventAggregator> eventAggregators, byte[] messageContent,
		             TypeDescriptor descriptor);
	}
}