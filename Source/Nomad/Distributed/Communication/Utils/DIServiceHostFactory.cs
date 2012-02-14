using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Castle.Windsor;

namespace Nomad.Distributed.Communication.Utils
{
	/// <summary>
	///		Implementation of <see cref="ServiceHostFactory"/> thats 
	/// uses the <see cref="IWindsorContainer"/>.
	/// </summary>
	public class DIServiceHostFactory : ServiceHostFactory
	{
		private readonly IWindsorContainer _container;

		public DIServiceHostFactory(IWindsorContainer container)
		{
			_container = container;
		}

		protected override ServiceHost CreateServiceHost(Type service, Uri[] baseAddresses)
		{
			return new DIServiceHost(service, _container, baseAddresses);
		}
	}
}