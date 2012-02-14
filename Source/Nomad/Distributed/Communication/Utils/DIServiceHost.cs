using System;
using System.ServiceModel;
using Castle.Windsor;

namespace Nomad.Distributed.Communication.Utils
{
	/// <summary>
	///		Implementation of <see cref="ServiceHost"/> which is aware of
	/// <see cref="IWindsorContainer"/>.
	/// </summary>
	public class DIServiceHost : ServiceHost
	{
		private IWindsorContainer _container;

		public DIServiceHost(IWindsorContainer container)
		{
			_container = container;
		}

		public DIServiceHost(Type serviceType, IWindsorContainer container, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
		{
			_container = container;
		}

		protected override void OnOpening()
		{
			Description.Behaviors.Add(new DIServiceBehaviour(_container));
			base.OnOpening();
		}
	}
}