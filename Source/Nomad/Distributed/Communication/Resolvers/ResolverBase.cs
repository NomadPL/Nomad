using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Nomad.Distributed.Communication.Resolvers
{
	/// <summary>
	///		Helper class providing mechanisms for creating 
	/// WCF enabled proxies.
	/// </summary>
	public class ResolverBase : IDisposable
	{
		protected DistributedConfiguration DistributedConfiguration;

		private IList<ChannelFactory<IDistributedEventAggregator>> _usedChannels;
		private IList<IDistributedEventAggregator> _usedIdeAs;

		public ResolverBase()
		{
			this._usedIdeAs = new List<IDistributedEventAggregator>();
			this._usedChannels = new List<ChannelFactory<IDistributedEventAggregator>>();
		}

		protected IDistributedEventAggregator CreateDEA(string url)
		{
			var channel = new ChannelFactory<IDistributedEventAggregator>(new NetTcpBinding(),
			                                                              new EndpointAddress(url));
			IDistributedEventAggregator idea = channel.CreateChannel();

			_usedIdeAs.Add(idea);
			_usedChannels.Add(channel);

			return idea;
		}

		public virtual void Dispose()
		{
			foreach (var channelFactory in _usedChannels)
			{
				channelFactory.Close();
			}
		}
	}
}