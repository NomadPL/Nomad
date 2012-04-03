using System;
using DistributableMessagesLibrary;
using Nomad.Communication.EventAggregation;
using Nomad.Messages.Loading;
using Nomad.Modules;

namespace DistributablePublisherModule
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
            _aggregator.Subscribe<NomadAllModulesLoadedMessage>(StartPublishing);
        }

        public void OnUnLoad()
        {
            // do nothing
        }

        #endregion

        private void StartPublishing(NomadAllModulesLoadedMessage obj)
        {
            Console.WriteLine("All modules loading, now publishing.");
            for (int i = 0; i < 5; i++)
            {
                string payload = "Sample Message " + i;
                var message = new DistributableMessage(payload);
                _aggregator.Publish(message);
            }
        }
    }
}
