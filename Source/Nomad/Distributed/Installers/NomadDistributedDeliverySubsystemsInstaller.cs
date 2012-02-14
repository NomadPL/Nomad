using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Nomad.Distributed.Communication.Deliveries.SingleDelivery;
using Nomad.Distributed.Communication.Deliveries.TimedDelivery;
using Nomad.Distributed.Communication.Deliveries.TopicDelivery;

namespace Nomad.Distributed.Installers
{
	/// <summary>
	///		Installs passed subsystem as single tons in the <see cref="WindsorContainer"/> 
	/// passed via <see cref="DistributedConfiguration"/>.
	/// </summary>
	public class NomadDistributedDeliverySubsystemsInstaller : IWindsorInstaller
	{
		private readonly DistributedConfiguration _distributedConfiguration;

		public NomadDistributedDeliverySubsystemsInstaller(DistributedConfiguration distributedConfiguration)
		{
			_distributedConfiguration = distributedConfiguration;
		}

		#region IWindsorInstaller Members

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<ITopicDeliverySubsystem>().Instance(_distributedConfiguration.TopicDeliverySubsystem),
				Component.For<ISingleDeliverySubsystem>().Instance(_distributedConfiguration.SingleDeliverySubsystem),
				Component.For<ITimedDeliverySubsystem>().Instance(_distributedConfiguration.TimedDeliverySubsystem));
		}

		#endregion
	}
}