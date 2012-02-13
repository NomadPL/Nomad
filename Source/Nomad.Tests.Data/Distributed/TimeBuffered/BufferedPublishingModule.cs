using System;
using Nomad.Communication.EventAggregation;
using Nomad.Messages.Loading;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.TimeBuffered
{
	/// <summary>
	///		Sample class used for publishing data
	/// </summary>
	public class BufferedPublishingModule : Nomad.Modules.IModuleBootstraper
	{
		private IEventAggregator _aggregator;
		private IEventAggregatorTicket<NomadAllModulesLoadedMessage> _allModulesLoadedSubscriptionTicket;

		public BufferedPublishingModule(IEventAggregator eventAggregator)
		{
			_aggregator = eventAggregator;
		}

		public void OnLoad()
		{
			for (int i = 0; i < 5; i++)
			{
				string payload = "Sample Message " + i;
				DistributableMessage message = new DistributableMessage(payload);
				_aggregator.PublishTimelyBuffered(message, DateTime.Now + new TimeSpan(0, 0, 1, 0));
			}
		}

		public void OnUnLoad()
		{
			// do nothing
		}
	}
}