using System;
using System.Collections.Generic;
using log4net;
using Nomad.Core;

namespace Nomad.Distributed.Communication.Deliveries.TopicDelivery
{
	public class BasicTopicDeliverySubsystem : ITopicDeliverySubsystem
	{
		private static readonly ILog Logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY,
		                                                           typeof (BasicTopicDeliverySubsystem));

		#region ITopicDeliverySubsystem Members

		public bool SentAll(IEnumerable<IDistributedEventAggregator> eventAggregators, byte[] messageContent,
		                    TypeDescriptor descriptor)
		{
			foreach (IDistributedEventAggregator dea in eventAggregators)
			{
				try
				{
					dea.OnPublish(messageContent, descriptor);
				}
				catch (Exception e)
				{
					Logger.Warn(string.Format("Could not sent message '{0}' to DEA: {1}", descriptor, dea), e);
				}
			}

			// using the reliable mechanisms of WCF devlivery is always succesfull to all proper processes
			return true;
		}

		#endregion
	}
}