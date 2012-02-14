using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Nomad.Communication.EventAggregation;
using Nomad.Distributed.Communication;
using Nomad.Distributed.Communication.Deliveries.SingleDelivery;
using Nomad.Distributed.Communication.Deliveries.TimedDelivery;
using Nomad.Distributed.Communication.Deliveries.TopicDelivery;

namespace Nomad.Distributed.Installers
{
	/// <summary>
	///		Installing the DEA on top of normal <see cref="EventAggregator"/>
	/// </summary>
	public class NomadDistributedEventAggregatorInstaller : IWindsorInstaller
	{
		public const String ON_SITE_NAME = "OnSiteEVG";

		#region IWindsorInstaller Members

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IGuiThreadProvider>().ImplementedBy<LazyWpfGuiThreadProvider>(),
				Component
					.For<IEventAggregator>()
					.Forward<DistributedEventAggregator>()
					.UsingFactoryMethod(
						x => new DistributedEventAggregator(new EventAggregator(container.Resolve<IGuiThreadProvider>()),
						                                    container.Resolve<ITopicDeliverySubsystem>(),
						                                    container.Resolve<ISingleDeliverySubsystem>(),
						                                    container.Resolve<ITimedDeliverySubsystem>()))
					.Named(ON_SITE_NAME)
					.LifeStyle.Singleton
				);
		}

		#endregion
	}
}