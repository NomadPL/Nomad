using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net;
using Nomad.Core;

namespace Nomad.Distributed.Communication.Resolvers
{
	/// <summary>
	///		Helper class providing mechanisms for creating 
	/// WCF enabled proxies.
	/// </summary>
	public class ResolverBase : IDisposable
	{
		private static readonly ILog Logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY,
		                                                           typeof (ResolverBase));

		private readonly IList<ChannelFactory<IDistributedEventAggregator>> _usedChannels;
		private readonly IList<IDistributedEventAggregator> _usedIdeAs;
		protected DistributedConfiguration DistributedConfiguration;

		public ResolverBase()
		{
			_usedIdeAs = new List<IDistributedEventAggregator>();
			_usedChannels = new List<ChannelFactory<IDistributedEventAggregator>>();
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			Logger.Debug("Disposing: " + typeof (ResolverBase));
			foreach (var channelFactory in _usedChannels)
			{
				try
				{
					channelFactory.Close();
				}
				catch (Exception e)
				{
					// NOTE: the exception must be eaten -> the cleanup show must go on
					Logger.Error("Exception during cleanup phase of channel",e);
				}
			}
		}

		#endregion

		protected IDistributedEventAggregator CreateDEA(string url)
		{
			var channel = new ChannelFactory<IDistributedEventAggregator>(new NetTcpBinding(),
			                                                              new EndpointAddress(url));
			IDistributedEventAggregator idea = channel.CreateChannel();

			_usedIdeAs.Add(idea);
			_usedChannels.Add(channel);

			return idea;
		}
	}
}