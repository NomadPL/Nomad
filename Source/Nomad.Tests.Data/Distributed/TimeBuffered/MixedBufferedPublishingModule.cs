using System;
using Nomad.Communication.EventAggregation;
using Nomad.Modules;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.TimeBuffered
{
    /// <summary>
    ///		Sample class used for publishing data
    /// </summary>
    public class MixedBufferedPublishingModule : IModuleBootstraper
    {
        private readonly IEventAggregator _aggregator;

        public MixedBufferedPublishingModule(IEventAggregator eventAggregator)
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
                _aggregator.PublishTimelyBuffered(message, DateTime.Now + new TimeSpan(0, 0, 1, 0));
                _aggregator.PublishTimelyBuffered(message, DateTime.MinValue);
            }
        }

        public void OnUnLoad()
        {
            // do nothing
        }

        #endregion
    }
}
