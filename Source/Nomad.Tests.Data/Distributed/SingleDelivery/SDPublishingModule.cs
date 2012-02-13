using Nomad.Communication.EventAggregation;
using Nomad.Modules;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.SingleDelivery
{
	/// <summary>
	///		Sample class used for publishing data
	/// </summary>
	public class SDPublishingModule : IModuleBootstraper
	{
		private readonly IEventAggregator _aggregator;

		public SDPublishingModule(IEventAggregator eventAggregator)
		{
			_aggregator = eventAggregator;
		}

		#region IModuleBootstraper Members

		public void OnLoad()
		{
			for (int i = 0; i < 5; i++)
			{
				string payload = "Sample Message " + i;
				DistributableMessage message = new DistributableMessage(payload);
				_aggregator.PublishSingleDelivery(message, SingleDeliverySemantic.AtLeastOnce);
			}
		}

		public void OnUnLoad()
		{
			// do nothing
		}

		#endregion
	}
}