using System;
using System.Collections.Generic;

namespace Nomad.Distributed.Communication.Resolvers
{
	/// <summary>
	///		Depending on <see cref="ResolutionMode"/> uses the
	/// proper version of the <see cref="IResolver"/> class.
	/// </summary>
	public class ResolverFactory : IResolver
	{
		private readonly DistributedConfiguration _configuration;
		private readonly SimpleResolver _simpleResolver;

		public ResolverFactory(DistributedConfiguration configuration)
		{
			_configuration = configuration;
			_simpleResolver = new SimpleResolver(configuration);
		}

		public IList<IDistributedEventAggregator> Resolve()
		{
			switch (_configuration.Mode)
			{
				case ResolutionMode.Manual:
					{
						return _simpleResolver.Resolve();
					}

				case ResolutionMode.External:
					{
						throw new NotImplementedException("The current version is not yet implemented");
					}
				case ResolutionMode.Service:
					{
						throw new NotImplementedException("The current versino is not yet implemented");
					}

				default:
					throw new InvalidOperationException("The provided mode was not found");
			}
		}

		public void Dispose()
		{
			_simpleResolver.Dispose();
		}
	}
}