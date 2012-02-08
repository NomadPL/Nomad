using System;
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
		public const String ON_SITE_NAME = "OnSiteEVG";
		public const String PURE_EA = "PureEVG";


		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IGuiThreadProvider>().ImplementedBy<LazyWpfGuiThreadProvider>(),
				Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().Named(PURE_EA).LifeStyle.Singleton,
				Component
					.For<IEventAggregator>()
					.UsingFactoryMethod(x => new DistributedEventAggregator(container.Resolve<IEventAggregator>()))
					.Named(ON_SITE_NAME)
					.LifeStyle.Singleton
				);
		}
	}
}