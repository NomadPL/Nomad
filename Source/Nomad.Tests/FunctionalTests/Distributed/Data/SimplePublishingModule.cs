using Nomad.Communication.EventAggregation;
using Nomad.Modules;

namespace Nomad.Tests.FunctionalTests.Distributed.Data
{
	/// <summary>
	///		Sample class used for publishing data
	/// </summary>
	public class SimplePublishingModule : IModuleBootstraper
	{
		private IEventAggregator _aggregator;

		public SimplePublishingModule(IEventAggregator eventAggregator)
		{
			_aggregator = eventAggregator;
		}

		public void OnLoad()
		{
			// TODO: maybe we should wait for all modules loaded
			for (int i = 0; i < 5; i++)
			{
				var payload = "Sample Message " + i;
				var message = new SharedInterfaces(payload);
				_aggregator.Publish(message);
			}
		}

		public void OnUnLoad()
		{
			// do nothing
		}
	}
}