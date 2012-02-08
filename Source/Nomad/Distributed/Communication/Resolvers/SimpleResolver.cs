using System;
using System.Collections.Generic;

namespace Nomad.Distributed.Communication.Resolvers
{
	/// <summary>
	///		Resolves the <see cref="IDistributedEventAggregator"/> instances using
	/// the <see cref="DistributedConfiguration.URLs"/>.
	/// </summary>
	public class SimpleResolver : IResolver
	{
		private readonly DistributedConfiguration _configuration;

		public SimpleResolver(DistributedConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IList<IDistributedEventAggregator> Resolve()
		{
			IList<IDistributedEventAggregator> deas = new List<IDistributedEventAggregator>(_configuration.URLs.Count);

			foreach (var url in _configuration.URLs)
			{
				// TODO: write creating proxy or something like this
			}

			return deas;
		}
	}
}