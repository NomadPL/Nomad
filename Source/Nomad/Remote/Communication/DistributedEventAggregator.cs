using System;
using System.Collections.Generic;
using Nomad.Communication.EventAggregation;
using Nomad.Messages;

namespace Nomad.Remote.Communication
{
	/// <summary>
	///     Distributed version of <see cref="IEventAggregator"/> which enables Nomad application
	/// communicate over WCF links.
	/// </summary>
	/// <remarks>
	///     This class is visible to the modules loaded to Nomad as simple <see cref="IEventAggregator"/>. 
	/// </remarks>
	public class DistributedEventAggregator : IEventAggregator, IDistributedEventAggregator
	{
		private readonly IEventAggregator _localEventAggregator;
		private readonly IList<IDistributedEventAggregator> deas;

		public DistributedEventAggregator(){}

		public DistributedEventAggregator(IList<IDistributedEventAggregator> deas, IEventAggregator localEventAggregator)
		{
			this.deas = deas;
			_localEventAggregator = localEventAggregator;
		}

		#region IDistributedEventAggregator Members

		public void OnPublish(NomadMessage message)
		{
			// propagate message to the local subscribers
			_localEventAggregator.Publish(message);
		}

		#endregion

		#region IEventAggregator Members

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action) where T : class
		{
			throw new NotImplementedException();
		}

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action, DeliveryMethod deliveryMethod) where T : class
		{
			throw new NotImplementedException();
		}

		public void Publish<T>(T message) where T : class
		{
			var m = message as NomadMessage;

			if (m == null)
			{
				throw new InvalidCastException();
			}

			// publish remote
			foreach (IDistributedEventAggregator dea in deas)
			{
				try
				{
					dea.OnPublish(m);
				}
				catch (Exception e)
				{
					// NOTE: eating exception here
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		#endregion
	}
}