using System;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Castle.Windsor;

namespace Nomad.Distributed.Communication.Utils
{
	/// <summary>
	///		Dependency Injection. Helper implementation of <see cref="IInstanceProvider"/> to
	/// fully use the <see cref="IWindsorContainer"/> from <see cref="Nomad"/>.
	/// </summary>
	public class DIInstanceProvider : IInstanceProvider
	{
		private readonly IWindsorContainer _container;
		private readonly Type _serviceType;

		public DIInstanceProvider(Type serviceType, IWindsorContainer container)
		{
			_serviceType = serviceType;
			_container = container;
		}

		#region IInstanceProvider Members

		public object GetInstance(InstanceContext instanceContext)
		{
			return GetInstance(instanceContext, null);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return _container.Resolve(_serviceType);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}

		#endregion
	}
}