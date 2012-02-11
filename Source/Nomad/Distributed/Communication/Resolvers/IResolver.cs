using System;
using System.Collections.Generic;

namespace Nomad.Distributed.Communication.Resolvers
{
	/// <summary>
	///		Resolves the already existing <see cref="DistributedEventAggregator"/> instances on system using 
	/// specified mode.
	/// </summary>
	public interface IResolver : IDisposable
	{
		IList<IDistributedEventAggregator> Resolve();
	}
}