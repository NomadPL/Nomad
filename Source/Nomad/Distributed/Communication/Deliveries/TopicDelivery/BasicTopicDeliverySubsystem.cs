using System;
using System.Collections.Generic;
using log4net;
using Nomad.Core;

namespace Nomad.Distributed.Communication.Deliveries.TopicDelivery
{
	/// <summary>
	///		Responsible for sending all the information to all subscribes. 
	/// <para>
	///		This is volatile version.
	/// </para>
	/// </summary>
	[Serializable]
	public class BasicTopicDeliverySubsystem : ITopicDeliverySubsystem
	{
		private ILog Logger;

		#region ITopicDeliverySubsystem Members

		public bool SentAll(IEnumerable<IDistributedEventAggregator> eventAggregators, byte[] messageContent,
		                    TypeDescriptor descriptor)
		{
			Logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY, typeof (BasicTopicDeliverySubsystem));

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