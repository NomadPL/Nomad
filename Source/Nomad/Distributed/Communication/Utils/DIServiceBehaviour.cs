using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Castle.Windsor;

namespace Nomad.Distributed.Communication.Utils
{
	/// <summary>
	///		Implementation of <see cref="IServiceBehavior"/> which enables
	/// use of Dependency Injection from <see cref="IWindsorContainer"/>
	/// </summary>
	public class DIServiceBehaviour : IServiceBehavior
	{
		private readonly IWindsorContainer _container;

		public DIServiceBehaviour(IWindsorContainer container)
		{
			_container = container;
		}

		#region IServiceBehavior Members

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
		                                 Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
			{
				var cd = cdb as ChannelDispatcher;
				if (cd != null)
				{
					foreach (EndpointDispatcher ed in cd.Endpoints)
					{
						ed.DispatchRuntime.InstanceProvider =
							new DIInstanceProvider(serviceDescription.ServiceType, _container);
					}
				}
			}
		}

		#endregion
	}
}