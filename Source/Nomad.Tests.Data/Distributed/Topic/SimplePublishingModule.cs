using Nomad.Communication.EventAggregation;
using Nomad.Modules;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.Topic
{
    /// <summary>
    ///		Sample class used for publishing data
    /// </summary>
    public class SimplePublishingModule : IModuleBootstraper
    {
        private readonly IEventAggregator _aggregator;

        public SimplePublishingModule(IEventAggregator eventAggregator)
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
                _aggregator.Publish(message);
            }
        }

        public void OnUnLoad()
        {
            // do nothing
        }

        #endregion
    }
}