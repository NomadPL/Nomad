namespace Nomad.Communication.EventAggregation
{
	/// <summary>
	/// Determines the semantic of <see cref="IEventAggregator.Publish{T}(T)"/> method.
	/// </summary>
	public enum SingleDeliverySemantic
	{
		/// <summary>
		/// Tries to deliver the message until the calling <see cref="IEventAggregator"/> does a successful delivery.
		/// On delivery exceptions tries to deliver to next available subscriber.
		/// </summary>
		AtLeastOnce,

		/// <summary>
		/// Tries to deliver the message until the calling <see cref="IEventAggregator"/> does a successful delivery or a delivery exception occurs.
		/// </summary>
		AtMostOnce
	}
}