﻿using System.Threading;
using Nomad.Communication.EventAggregation;
using Nomad.Communication.ServiceLocation;
using Nomad.Messages.Loading;
using Nomad.Modules;
using Nomad.Regions;

namespace ListingAllLoadedModule
{
    public class ListingAllLoadedModule : IModuleBootstraper
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IServiceLocator _serviceLocator;
        private RegionManager _regionManager;


        public ListingAllLoadedModule(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _eventAggregator = serviceLocator.Resolve<IEventAggregator>();
        }

        #region IModuleBootstraper Members

        public void OnLoad()
        {
            // make this delivery possible in any thread
            _eventAggregator.Subscribe<NomadAllModulesLoadedMessage>(AllModulesLoaded,
                                                                     DeliveryMethod.GuiThread);
        }


        public void OnUnLoad()
        {
        }

        #endregion

        private void AllModulesLoaded(NomadAllModulesLoadedMessage obj)
        {
            var listingControl = new ListingControl(_serviceLocator);
            var regionManager = _serviceLocator.Resolve<RegionManager>();
            var region = regionManager.GetRegion("rightSideMenu");
            region.AddView(listingControl);
        }
    }
}