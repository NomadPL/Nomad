using Nomad.Communication.EventAggregation;
using Nomad.Messages.Loading;

namespace Nomad.Tests.Data.Distributed.Topic
{
	/// <summary>
	///		Sample class used for publishing data
	/// </summary>
	public class SimplePublishingModule : Nomad.Modules.IModuleBootstraper
	{
		private IEventAggregator _aggregator;
		private IEventAggregatorTicket<NomadAllModulesLoadedMessage> _allModulesLoadedSubscriptionTicket;

		public SimplePublishingModule(IEventAggregator eventAggregator)
		{
			_aggregator = eventAggregator;
		}

		public void OnLoad()
		{
			for (int i = 0; i < 5; i++)
			{
				string payload = "Sample Message " + i;
				DistributableMessage message = new DistributableMessage(payload);
				_aggregator.Publish(message);
			}
		}

		public void OnUnLoad()
		{
			// do nothing
		}
	}
}
