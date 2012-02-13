using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Nomad.Communication.EventAggregation;
using Nomad.Distributed.Communication;
using Nomad.Distributed.Communication.Deliveries.TopicDelivery;

namespace Nomad.Distributed.Installers
{
	/// <summary>
	///		Installing the DEA on top of normal <see cref="EventAggregator"/>
	/// </summary>
	public class NomadDistributedEventAggregatorInstaller : IWindsorInstaller
	{
		public const String ON_SITE_NAME = "OnSiteEVG";


		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			// TODO: installations of delivery subsystems should be configured within distributed configuration
			container.Register(
				Component.For<IGuiThreadProvider>().ImplementedBy<LazyWpfGuiThreadProvider>(),
				Component.For<ITopicDeliverySubsystem>().ImplementedBy<BasicTopicDeliverySubsystem>(),
				Component
					.For<IEventAggregator>()
					.UsingFactoryMethod(x => new DistributedEventAggregator(new EventAggregator(container.Resolve<IGuiThreadProvider>()),
																								container.Resolve<ITopicDeliverySubsystem>()))
					.Named(ON_SITE_NAME)
					.LifeStyle.Singleton
				);
		}
	}
}