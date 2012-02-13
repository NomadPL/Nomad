using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using log4net;
using Nomad.Communication.EventAggregation;
using Nomad.Core;
using Nomad.Distributed.Communication.Deliveries.TopicDelivery;
using Nomad.Distributed.Communication.Utils;
using Nomad.Distributed.Installers;
using Nomad.Messages;
using Nomad.Messages.Distributed;
using Version = Nomad.Utils.Version;

namespace Nomad.Distributed.Communication
{
	/// <summary>
	///     Distributed version of <see cref="IEventAggregator"/> which enables Nomad application
	/// communicate over WCF links.
	/// </summary>
	/// <remarks>
	///     This class is visible to the modules loaded to Nomad as simple <see cref="IEventAggregator"/>. 
	/// This class uses various subsystems for dispatching the proper behaviour during the publishing new messages.
	/// TODO: write about the subsystems
	/// </remarks>
	public class DistributedEventAggregator : MarshalByRefObject,
	                                          IEventAggregator, IDistributedEventAggregator, IDisposable
	{
		private static readonly ILog Loggger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY,
		                                                            typeof (DistributedEventAggregator));

		private static IEventAggregator _localEventAggregator;


		private static readonly object LockObject = new object();
		private readonly ITopicDeliverySubsystem _topicDelivery;

		private IList<IDistributedEventAggregator> _deas;


		/// <summary>
		///     This constructor is needed by the <see cref="ServiceHost"/>.
		/// </summary>
		public DistributedEventAggregator()
		{
		}

		/// <summary>
		///		This constructor is used by container during initalization. Used by <see cref="NomadDistributedEventAggregatorInstaller"/>
		/// </summary>
		/// <param name="localEventAggrgator">The local event aggregator used by </param>
		///<param name="topicDelivery">The subsystem responsible for topic deliveries</param>
		public DistributedEventAggregator(IEventAggregator localEventAggrgator, ITopicDeliverySubsystem topicDelivery)
		{
			_topicDelivery = topicDelivery;
			LocalEventAggregator = localEventAggrgator;
		}

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
		///     Changes the list of the registerd remote site for the DEA.
		/// </summary>
		public IList<IDistributedEventAggregator> RemoteDistributedEventAggregator
		{
			set { _deas = new List<IDistributedEventAggregator>(value); }
			get { return _deas; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			try
			{
				// onDispose method or sending Dispose message through communication normal services
				var msg = new NomadDetachingMessage("Sample Dispatching Message");
				SendToAllControl(msg);
			}
			catch (Exception e)
			{
				// NOTE: the exception must be eaten -> the cleanup show must go on
				Loggger.Error("There was serious error during the cleanup phase", e);
			}
		}

		#endregion

		#region IDistributedEventAggregator Members

		public void OnPublishControl(NomadDistributedMessage message)
		{
			Loggger.Debug(string.Format("Acquired message {0}", message));

			// propagate message to the local subscribers
			LocalEventAggregator.Publish(message);

			// do not propagete to the other DEA
			// at least in this implementation
		}

		public void OnPublish(byte[] byteStream, TypeDescriptor typeDescriptor)
		{
			Loggger.Debug(string.Format("Acquired message of type {0}", typeDescriptor));

			// TODO: this code should invoke one of the subsystems to be working good
			try
			{
				// try recreating this type 
				Type type = Type.GetType(typeDescriptor.QualifiedName);
				if (type != null)
				{
					var nomadVersion = new Version(type.Assembly.GetName().Version);

					if (!nomadVersion.Equals(typeDescriptor.Version))
					{
						throw new InvalidCastException("The version of the assembly does not match");
					}
				}

				// try deserializing object
				Object sendObject = MessageSerializer.Deserialize(byteStream);

				// check if o is assignable
				if (type != null && !type.IsInstanceOfType(sendObject))
				{
					throw new InvalidCastException("The sent object cannot be casted to sent type");
				}

				// invoke this generic method with type t
				// TODO: this is totaly not refactor aware use expression tree to get this publish thing
				MethodInfo methodInfo = LocalEventAggregator.GetType().GetMethod("Publish");
				MethodInfo goodMethodInfo = methodInfo.MakeGenericMethod(type);

				goodMethodInfo.Invoke(LocalEventAggregator, new[] {sendObject});
			}
			catch (Exception e)
			{
				Loggger.Warn("The type not be recreated", e);
			}
		}

		public void OnPublishSingleDelivery(byte[] byteStream, TypeDescriptor typeDescriptor)
		{
			throw new NotImplementedException();
		}

		public void OnPublishTimelyDelivery(byte[] byteSteam, TypeDescriptor typeDescriptor, DateTime voidTime)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEventAggregator Members

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action) where T : class
		{
			return Subscribe(action, DeliveryMethod.AnyThread);
		}

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action, DeliveryMethod deliveryMethod) where T : class
		{
			// subscribe on local event
			return _localEventAggregator.Subscribe(action, deliveryMethod);

			// subscribe on remote or not by now
		}

		public bool Publish<T>(T message) where T : class
		{
			// try publishing message in the local system on this machine
			bool delivered = _localEventAggregator.Publish(message);

			// filter local NomadMessage
			if (message is NomadMessage)
			{
				return true;
			}
			
			// try publishing message in the remote system
			byte[] bytes = MessageSerializer.Serialize(message);
			if (bytes == null)
				return false;

			var descriptor = new TypeDescriptor(message.GetType());

			bool remoteDelivered = _topicDelivery.SentAll(RemoteDistributedEventAggregator, bytes, descriptor);
			
			if (remoteDelivered && delivered)
			{
				return true;
			}

			return delivered;
		}

		public bool PublishTimelyBuffered<T>(T message, DateTime validUntil) where T : class
		{
			throw new NotImplementedException();
		}

		public bool PublishSingleDelivery<T>(T message, SingleDeliverySemantic singleDeliverySemantic) where T : class
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		///		Sends the message using control type of invocation. This code works
		/// for now perfectly fine.
		/// </summary>
		private void SendToAllControl<T>(T message)
		{
			// NOTE: this code should be parralelized
			foreach (IDistributedEventAggregator dea in _deas)
			{
				try
				{
					dea.OnPublishControl(message as NomadDistributedMessage);
				}
				catch (Exception e)
				{
					Loggger.Warn("Exception during sending to DEA", e);
					throw;
				}
			}
		}
	}
}