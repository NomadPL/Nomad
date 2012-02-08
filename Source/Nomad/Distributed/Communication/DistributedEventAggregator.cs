using System;
using System.Collections.Generic;
using System.ServiceModel;
using Nomad.Communication.EventAggregation;
using Nomad.Messages;
using Nomad.Messages.Distributed;

namespace Nomad.Distributed.Communication
{
	/// <summary>
	///     Distributed version of <see cref="IEventAggregator"/> which enables Nomad application
	/// communicate over WCF links.
	/// </summary>
	/// <remarks>
	///     This class is visible to the modules loaded to Nomad as simple <see cref="IEventAggregator"/>. 
	/// </remarks>
	public class DistributedEventAggregator : MarshalByRefObject,
											  IEventAggregator, IDistributedEventAggregator, IDisposable
	{
		private IEventAggregator _localEventAggregator;
		private IList<IDistributedEventAggregator> _deas;

		/// <summary>
		///     This constructor is needed by the <see cref="ServiceHost"/>.
		/// </summary>
		public DistributedEventAggregator()
		{
		}

		#region IDistributedEventAggregator Members

		public void OnPublish(NomadMessage message)
		{
			// propagate message to the local subscribers
			_localEventAggregator.Publish(message);

			// TODO: maybe log message being saved or something
		}

		#endregion

		/// <summary>
		///     Changes the list of the registerd remote site for the DEA.
		/// </summary>
		public IList<IDistributedEventAggregator> RemoteDistributedEventAggregator
		{
			set { _deas = value; }
			get { return _deas; }
		}

		/// <summary>
		///     Gets or sets the Local Event Aggregator.
		/// </summary>
		/// <remarks>
		///     The local event aggregator can be set <c>only once</c>.
		/// </remarks>
		public IEventAggregator LocalEventAggregator
		{
			get { return _localEventAggregator; }
			set
			{
				if(_localEventAggregator !=null)
					throw new InvalidOperationException("The local event aggregator can be registered only once");

				_localEventAggregator = value;
			}
		}

		#region IEventAggregator Members

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action) where T : class
		{
			return this.Subscribe(action, DeliveryMethod.AnyThread);
		}

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action, DeliveryMethod deliveryMethod) where T : class
		{
			// subscribe on local event
			return _localEventAggregator.Subscribe(action, deliveryMethod);

			// subscribe on remote or not by now
		}

		public void Publish<T>(T message) where T : class
		{
			// try publishing message int the local system on this machine
			_localEventAggregator.Publish(message);

			var m = message as NomadMessage;
			if (m == null)
			{
				throw new InvalidCastException();
			}

			// publish remote
			SendToAll(m);
		}

		#endregion

		private void SendToAll(NomadMessage message)
		{
			// NOTE: this code should be parralelized
			foreach (IDistributedEventAggregator dea in _deas)
			{
				try
				{
					dea.OnPublish(message);
				}
				catch (Exception e)
				{
					// NOTE: eating exception here, needs the logger 
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		public void Dispose()
		{
			// onDispose method or sending Dispos message throu communication normal services
			var msg = new NomadDetachingMessage("Sample Dispatching Message");
			SendToAll(msg);
		}
	}
}