using System;
using System.ServiceModel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Nomad.Communication.EventAggregation;
using Nomad.Distributed.Communication;
using Nomad.Distributed.Communication.Resolvers;

namespace Nomad.Distributed
{
	/// <summary>
	///		Installing the DEA on top of normal <see cref="EventAggregator"/>
	/// </summary>
	public class NomadDistributedEventAggregatorInstaller : IWindsorInstaller
	{
		public const String ON_SITE_NAME ="OnSiteEVG";
		public const String PURE_EA = "PureEVG";

		private readonly DistributedConfiguration _distributedConfiguration;
		private ServiceHost _distributedEventAggregatorServiceHost;
		private DistributedEventAggregator _localDEA;

		public NomadDistributedEventAggregatorInstaller(DistributedConfiguration distributedConfiguration)
		{
			_distributedConfiguration = distributedConfiguration;
		}

		private DistributedEventAggregator RegisterServiceHostDEA(IWindsorContainer containern)
		{
			_distributedEventAggregatorServiceHost = new ServiceHost(typeof(DistributedEventAggregator));
			_distributedEventAggregatorServiceHost.AddServiceEndpoint
				(
					typeof(IDistributedEventAggregator),
					_distributedConfiguration.LocalBinding,
					_distributedConfiguration.LocalURI
				);

			var resolver = new ResolverFactory(_distributedConfiguration);

			_localDEA = (DistributedEventAggregator)_distributedEventAggregatorServiceHost.SingletonInstance;
			_localDEA.RemoteDistributedEventAggregator = resolver.Resolve();
			_localDEA.LocalEventAggregator = containern.Resolve<IEventAggregator>(PURE_EA);

			_distributedEventAggregatorServiceHost.Open();

			return _localDEA;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IGuiThreadProvider>().ImplementedBy<LazyWpfGuiThreadProvider>(),
				Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().Named(PURE_EA).LifeStyle.Singleton,
				Component
					.For<IEventAggregator>()
					.Instance(RegisterServiceHostDEA(container))
					.Named(ON_SITE_NAME)
					.LifeStyle.Singleton
					);
		}
	}
}