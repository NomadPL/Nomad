using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Nomad.Communication.EventAggregation;
using Nomad.Core;
using Nomad.Distributed.Communication.Deliveries.SingleDelivery;
using Nomad.Distributed.Communication.Deliveries.TimedDelivery;
using Nomad.Distributed.Communication.Deliveries.TopicDelivery;
using Nomad.Distributed.Communication.Utils;
using Nomad.Distributed.Installers;
using Nomad.Messages;
using Nomad.Messages.Distributed;
using Nomad.Messages.Loading;

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
		private static readonly ILog logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY,
		                                                           typeof (DistributedEventAggregator));

		private static readonly IDictionary<string, int> TicketsCounter = new Dictionary<string, int>();
		private readonly IEventAggregator _localEventAggregator;


		private readonly ISingleDeliverySubsystem _singleDelivery;
		private readonly ITimedDeliverySubsystem _timedDeliverySubsystem;
		private readonly ITopicDeliverySubsystem _topicDelivery;

		private IList<IDistributedEventAggregator> _deas;


		/// <summary>
		///		This constructor is used by container during initalization. Used by <see cref="NomadDistributedEventAggregatorInstaller"/>
		/// </summary>
		///<param name="localEventAggregator"></param>
		///<param name="topicDelivery">The subsystem responsible for topic deliveries if <see cref="IEventAggregator"/></param>
		///<param name="singleDelivery">The subsystem used for single delivery method of <see cref="IEventAggregator"/></param>
		///<param name="timedDeliverySubsystem"></param>
		public DistributedEventAggregator(IEventAggregator localEventAggregator, ITopicDeliverySubsystem topicDelivery,
		                                  ISingleDeliverySubsystem singleDelivery,
		                                  ITimedDeliverySubsystem timedDeliverySubsystem)
		{
			_localEventAggregator = localEventAggregator;
			_topicDelivery = topicDelivery;
			_singleDelivery = singleDelivery;
			_timedDeliverySubsystem = timedDeliverySubsystem;
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
				logger.Error("There was serious error during the cleanup phase", e);
			}
		}

		#endregion

		#region IDistributedEventAggregator Members

		public void OnPublishControl(NomadDistributedMessage message)
		{
			logger.Debug(string.Format("Acquired message {0}", message));

			// propagate message to the local subscribers
			_localEventAggregator.Publish(message);

			// do not propagete to the other DEA
			// at least in this implementation
		}

		public bool OnPublish(byte[] byteStream, TypeDescriptor typeDescriptor)
		{
			logger.Debug(string.Format("Acquired message of type {0}", typeDescriptor));

			try
			{
				// try recreating this type 
				object sendObject;
				Type type;
				MessageSerializer.UnPackData(typeDescriptor, byteStream, out sendObject, out type);

				// invoke this generic method with type t
				// TODO: this is totaly not refactor aware use expression tree to get this publish thing
				MethodInfo methodInfo = _localEventAggregator.GetType().GetMethod("Publish");
				MethodInfo goodMethodInfo = methodInfo.MakeGenericMethod(type);
				goodMethodInfo.Invoke(_localEventAggregator, new[] {sendObject});
			}
			catch (Exception e)
			{
				logger.Warn("The type not be recreated", e);
			}

			// NOTE: this is the role of subsystem to answer 
			return true;
		}


		public bool OnPublishSingleDelivery(byte[] byteStream, TypeDescriptor typeDescriptor)
		{
			logger.Debug(string.Format("Acquired single delivery message of type {0}", typeDescriptor));

			// try recreating this type 
			object sendObject;
			Type type;
			try
			{
				MessageSerializer.UnPackData(typeDescriptor, byteStream, out sendObject, out type);
			}
			catch (Exception e)
			{
				logger.Warn("The type not be recreated", e);
				return false;
			}

			bool deliveryStatus = _singleDelivery.RecieveSingle(_localEventAggregator, sendObject, type);
			return deliveryStatus;
		}

		public void OnPublishTimelyBufferedDelivery(byte[] byteStream, TypeDescriptor typeDescriptor, DateTime voidTime)
		{
			logger.Debug(string.Format("Acquired timelyBuffered message of type {0} valid until {1}", typeDescriptor, voidTime));

			try
			{
				//try recreating this type 
				object sendObject;
				Type type;
				MessageSerializer.UnPackData(typeDescriptor, byteStream, out sendObject, out type);

				// invoke this generic method with type T
				MethodInfo methodInfo = _localEventAggregator.GetType().GetMethod("PublishTimelyBuffered");
				MethodInfo goodMethodInfo = methodInfo.MakeGenericMethod(type);

				goodMethodInfo.Invoke(_localEventAggregator, new[] {sendObject, voidTime});
			}
			catch (Exception e)
			{
				//logger.Warn("The type of the timelyBuffered message could not be recreated", e);
				// adding message to binaryMessagesBufer
				_timedDeliverySubsystem.AddMessageToBuffer(new BufferedBinaryMessage(byteStream, voidTime, typeDescriptor));
			}
		}

		public bool IsSubscriberForType(TypeDescriptor descriptor)
		{
			string value = descriptor.QualifiedName;
			int result;
			if (TicketsCounter.TryGetValue(value, out result) && result > 0)
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Data Packing

		// NOTE: this code could be easly refactored into other class (quite the same as serialization / desrialization)

		private void PackData<T>(T message, out byte[] bytes, out TypeDescriptor descriptor)
		{
			// adding binaryBuffer deserializability possibility check
			if (message is NomadAllModulesLoadedMessage)
			{
				_timedDeliverySubsystem.TryDeliverBufferedMessages(_localEventAggregator);
			}

			if (message is NomadMessage)
			{
				bytes = null;
				descriptor = null;
				return;
			}
			bytes = MessageSerializer.Serialize(message);
			descriptor = new TypeDescriptor(message.GetType());
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
			IEventAggregatorTicket<T> ticket = _localEventAggregator.Subscribe(action, deliveryMethod);

			// update counter of subscrbers
			int value;
			string key = typeof (T).AssemblyQualifiedName;
			if (TicketsCounter.TryGetValue(key, out value))
			{
				TicketsCounter[key] = value + 1;
			}
			else
			{
				TicketsCounter[key] = 1;
			}

			// remove from ticketCounter if ticket is disposed
			ticket.TicketDisposed += (sender, args) =>
			                         	{
			                         		int v;
			                         		string k = args.EventType.AssemblyQualifiedName;
			                         		if (TicketsCounter.TryGetValue(k, out v))
			                         		{
			                         			TicketsCounter[k] = v - 1;
			                         		}
			                         		else
			                         		{
			                         			throw new InvalidOperationException("Removed the typed which was never added");
			                         		}
			                         	};

			// subscribe on remote or not by now);
			return ticket;
		}

		public bool Publish<T>(T message) where T : class
		{
			// try publishing message in the local system on this machine
			bool delivered = _localEventAggregator.Publish(message);

			// prepare for publishing remotely
			byte[] bytes;
			TypeDescriptor descriptor;
			try
			{
				PackData(message, out bytes, out descriptor);
				if (bytes == null && descriptor == null)
				{
					// tried to send NomadMessage.
					return false;
				}
			}
			catch (Exception e)
			{
				logger.Warn("Could not preapre message for sending", e);
				return false;
			}

			bool remoteDelivered = _topicDelivery.SentAll(RemoteDistributedEventAggregator, bytes, descriptor);

			return delivered && remoteDelivered;
		}

		public void PublishTimelyBuffered<T>(T message, DateTime validUntil) where T : class
		{
			// try publishing message in the local system on this machine
			_localEventAggregator.PublishTimelyBuffered(message, validUntil);

			// prepare for publishing remotely
			byte[] bytes;
			TypeDescriptor descriptor;
			try
			{
				PackData(message, out bytes, out descriptor);
				if (bytes == null && descriptor == null)
				{
					// tried to send NomadMessage.
					return;
				}
			}
			catch (Exception e)
			{
				logger.Warn("Could not preapre message for sending", e);
				return;
			}

			// TODO: add _timelyBufferedDelivery ;P
			// deliverying to remote sites
			foreach (IDistributedEventAggregator dea in _deas)
			{
				try
				{
					dea.OnPublishTimelyBufferedDelivery(bytes, descriptor, validUntil);
				}
				catch (Exception e)
				{
					logger.Warn(
						string.Format("Could not sent timelyBuffered Message '{0}' valid until {2} to DEA: {1}", descriptor, dea,
						              validUntil), e);
				}
			}
		}

		public bool PublishSingleDelivery<T>(T message, SingleDeliverySemantic singleDeliverySemantic) where T : class
		{
			bool localDelivery = _localEventAggregator.PublishSingleDelivery(message, singleDeliverySemantic);
			if (localDelivery)
				return true;

			// prepare for publishing remotely
			byte[] bytes;
			TypeDescriptor descriptor;
			try
			{
				PackData(message, out bytes, out descriptor);
			}
			catch (Exception e)
			{
				// we return false because the delivery is not possible at all in any semantics
				logger.Warn("Could not preapre message for sending", e);
				return false;
			}

			bool remoteDelivery = _singleDelivery.SentSingle(RemoteDistributedEventAggregator, bytes, descriptor,
			                                                 singleDeliverySemantic);
			return remoteDelivery;
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
					logger.Warn("Exception during sending to DEA", e);
					throw;
				}
			}
		}
	}
}