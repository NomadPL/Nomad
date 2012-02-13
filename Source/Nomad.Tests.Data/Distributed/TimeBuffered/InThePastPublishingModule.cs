using System;
using Nomad.Communication.EventAggregation;
using Nomad.Messages.Loading;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.TimeBuffered
{
	/// <summary>
	///		Sample class used for publishing data
	/// </summary>
	public class InThePastPublishingModule : Nomad.Modules.IModuleBootstraper
	{
		private IEventAggregator _aggregator;
		private IEventAggregatorTicket<NomadAllModulesLoadedMessage> _allModulesLoadedSubscriptionTicket;

		public InThePastPublishingModule(IEventAggregator eventAggregator)
		{
			_aggregator = eventAggregator;
		}

		public void OnLoad()
		{
			for (int i = 0; i < 5; i++)
			{
				string payload = "Sample Message " + i;
				DistributableMessage message = new DistributableMessage(payload);
				_aggregator.PublishTimelyBuffered(message, DateTime.Now);
			}
		}

		public void OnUnLoad()
		{
			// do nothing
		}
	}
}
