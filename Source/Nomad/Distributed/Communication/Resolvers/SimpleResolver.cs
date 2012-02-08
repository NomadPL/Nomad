using System;
using System.Collections.Generic;

namespace Nomad.Distributed.Communication.Resolvers
{
	/// <summary>
	///		Resolves the <see cref="IDistributedEventAggregator"/> instances using
	/// the <see cref="DistributedConfiguration.URLs"/>.
	/// </summary>
	public class SimpleResolver : ResolverBase, IResolver
	{
		public SimpleResolver(DistributedConfiguration distributedConfiguration)
		{
			DistributedConfiguration = distributedConfiguration;
		}

		public IList<IDistributedEventAggregator> Resolve()
		{
			IList<IDistributedEventAggregator> deas = new List<IDistributedEventAggregator>(DistributedConfiguration.URLs.Count);

			foreach (var url in DistributedConfiguration.URLs)
			{
				IDistributedEventAggregator dea = CreateDEA(url);
				deas.Add(dea);
			}

			return deas;
		}
	}
}