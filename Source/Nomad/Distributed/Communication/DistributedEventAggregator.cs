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
		private static IEventAggregator _localEventAggregator;
		private IList<IDistributedEventAggregator> _deas;

		private static readonly object LockObject = new object();

		private static IEventAggregator LocalEventAggregator
		{
			get { return _localEventAggregator; }
			set
			{
				lock (LockObject)
				{
					if (_localEventAggregator != null)
						throw new InvalidOperationException("The local event aggregator can be set only once");

					_localEventAggregator = value;
				}
			}
		}

		/// <summary>
		///     This constructor is needed by the <see cref="ServiceHost"/>.
		/// </summary>
		public DistributedEventAggregator()
		{
		}

		/// <summary>
		///		This constructor is used by container during initalization. Used by <see cref="NomadDistributedEventAggregatorInstaller"/>
		/// </summary>
		/// <param name="localEventAggrgator"></param>
		public DistributedEventAggregator(IEventAggregator localEventAggrgator)
		{
			LocalEventAggregator = localEventAggrgator;
		}

		#region IDistributedEventAggregator Members

		public void OnPublish(NomadMessage message)
		{
			// propagate message to the local subscribers
			LocalEventAggregator.Publish(message);

			// TODO: maybe log message being saved or something
			
			Console.WriteLine("Received message:"+message.Message);
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

		
			// publish remote
			SendToAll(message);
		}

		#endregion

		private void SendToAll<T>(T message)
		{
			// NOTE: this code should be parralelized
			foreach (IDistributedEventAggregator dea in _deas)
			{
				try
				{
					dea.OnPublish(message as NomadMessage);
				}
				catch (Exception e)
				{
					// NOTE: eating exception here, needs the logger 
					throw e;
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		public void Dispose()
		{
			// onDispose method or sending Dispos message throu communication normal services
			//var msg = new NomadDetachingMessage("Sample Dispatching Message");
		//	SendToAll(msg);
		}
	}
}