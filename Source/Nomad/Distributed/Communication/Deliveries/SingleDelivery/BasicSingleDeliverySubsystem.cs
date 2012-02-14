using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Nomad.Communication.EventAggregation;
using Nomad.Core;

namespace Nomad.Distributed.Communication.Deliveries.SingleDelivery
{
	/// <summary>
	///		The simplest possible implementation of <see cref="ISingleDeliverySubsystem"/>. The messages
	/// are volatile (if no subscribers are avaliable the message is lost). 
	/// 
	/// <para> 
	/// Algorithm used to find out the node to which the message is about to be sent is the <c>Tyran Algorithm</c>
	/// based on FIFO responses. 
	/// </para>
	/// <para>
	///		This code follows the semantics of the <see cref="SingleDeliverySemantic.AtLeastOnce"/>. This is default 
	/// implementation of <see cref="ISingleDeliverySubsystem"/>.
	/// </para>
	/// </summary>
	public class BasicSingleDeliverySubsystem : ISingleDeliverySubsystem
	{
		private static readonly ILog Logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY,
		                                                           typeof (BasicSingleDeliverySubsystem));

		#region ISingleDeliverySubsystem Members

		public bool SentSingle(IEnumerable<IDistributedEventAggregator> eventAggregators, byte[] messageContent,
		                       TypeDescriptor descriptor)
		{
			var deaWithSubscribers = new List<IDistributedEventAggregator>();

			// at fist send to the other deas the is subscrbed
			// NOTE: this phase can be parallelized
			foreach (IDistributedEventAggregator dea in eventAggregators)
			{
				try
				{
					if (dea.IsSubscriberForType(descriptor))
					{
						deaWithSubscribers.Add(dea);
					}
				}
				catch (Exception e)
				{
					// do nothing except for logging such exception
					Logger.Warn("Exception during sending IsSubscrier", e);
				}
			}

			if (deaWithSubscribers.Count == 0)
			{
				// no message can be delivered caouse no one waits for it
				return false;
			}

			// deliver the messages using this version
			foreach (IDistributedEventAggregator dea in deaWithSubscribers)
			{
				try
				{
					bool result = dea.OnPublishSingleDelivery(messageContent, descriptor);
					if (result)
					{
						// TODO: add hacking with sementaics etc
						// hacking with semantics ?
						return true;
					}
				}
				catch (Exception e)
				{
					Logger.Warn("Exception during sending OnPublish phase", e);
				}
			}

			// nothing succeeded
			return false;
		}

		public bool RecieveSingle(IEventAggregator eventAggregator, object sendObject, Type type)
		{
			// TODO: provide lambda expression reader from some trick in Moku
			var methodInfo = eventAggregator.GetType().GetMethod("PublishSingleDelivery");
			var goodMethodInfo = methodInfo.MakeGenericMethod(type);

			// semantic is not used for local event aggregator
			object result = goodMethodInfo.Invoke(eventAggregator, new[] { sendObject , null});
			return (bool) result;
		}

		#endregion
	}
}