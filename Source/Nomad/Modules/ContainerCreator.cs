﻿using System;
using System.Reflection;
using System.ServiceModel;
using Castle.Windsor;
using log4net;
using log4net.Repository;
using Nomad.Communication.EventAggregation;
using Nomad.Communication.ServiceLocation;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Distributed.Communication;
using Nomad.Distributed.Communication.Resolvers;
using Nomad.Distributed.Communication.Utils;
using Nomad.Distributed.Installers;
using Nomad.Modules.Installers;
using Nomad.Utils;

namespace Nomad.Modules
{
	/// <summary>
	///     Class responsible for creation of the main IoC container in Nomad.
	/// </summary>
	/// <remarks>
	/// <para>
	///     This class is marshal able because its main purpose is to provide way for <see cref="NomadKernel"/> 
	///     to initialize the Nomad framework on the module side of appDomain.
	/// </para>
	/// <para>
	///     As default implementation of <see cref="IWindsorContainer"/> is used default WindsorContainer.
	/// </para>
	/// </remarks>
	public class ContainerCreator : MarshalByRefObject, IDisposable
	{
		private readonly IWindsorContainer _windsorContainer;
		private ServiceHost _distributedEventAggregatorServiceHost;

		private ILog _logger;
		private ILoggerRepository _repository;
		private IResolver _resolver;


		/// <summary>
		///     Initializes new instance of the <see cref="ContainerCreator"/> class.
		/// </summary>
		/// <remarks>
		///     Initializes new instance of <see cref="WindsorContainer"/> with default implementation.
		/// </remarks>
		public ContainerCreator()
		{
			_windsorContainer = new WindsorContainer();
		}


		/// <summary>
		///     IWindsor container which works as main backend.
		/// </summary>
		public IWindsorContainer WindsorContainer
		{
			get { return _windsorContainer; }
		}

		/// <summary>
		///     Gets the object implementing <see cref="IEventAggregator"/> class. 
		/// </summary>        
		public IEventAggregator EventAggregatorOnModulesDomain
		{
			get { return _windsorContainer.Resolve<IEventAggregator>("OnSiteEVG"); }
		}

		/// <summary>
		///     Gets the object implementing <see cref="IServiceLocator"/> class.
		/// </summary>
		public IServiceLocator ServiceLocator
		{
			get { return _windsorContainer.Resolve<IServiceLocator>(); }
		}

		#region IDisposable Members

		public void Dispose()
		{
			_logger.Debug("Disposing: " + typeof(ContainerCreator));

			if (_distributedEventAggregatorServiceHost != null)
			{
				_distributedEventAggregatorServiceHost.Close();
			    _distributedEventAggregatorServiceHost = null;
			}

			if (_resolver != null)
			{
				_resolver.Dispose();
			    _resolver = null;
			}

			_logger.Debug("Shutting down logger repository");
			if (_repository != null)
			{
				_repository.Shutdown();
			    _repository = null;
			}
		}

		#endregion

		/// <summary>
		///     Initializes new instance of the <see cref="ModuleLoader"/> class as an implementation of <see cref="IModuleLoader"/>
		/// </summary>
		/// <remarks>
		///     The created class is dependent on <see cref="WindsorContainer"/>, which is injected during construction.
		/// </remarks>
		/// <returns>
		///     New instance of <see cref="ModuleLoader"/> class.
		/// </returns>
		public IModuleLoader CreateModuleLoaderInstance()
		{
			return _windsorContainer.Resolve<IModuleLoader>();
		}

		private void RegisterServiceHostDEA(DistributedConfiguration distributedConfiguration)
		{
			_distributedEventAggregatorServiceHost = new DIServiceHost(typeof (DistributedEventAggregator), _windsorContainer);
			_distributedEventAggregatorServiceHost.AddServiceEndpoint
				(
					typeof (IDistributedEventAggregator),
					new NetTcpBinding(),
					distributedConfiguration.LocalURI
				);
			_distributedEventAggregatorServiceHost.Open();
		}

		public void Install(DistributedConfiguration distributedConfiguration, string loggerConfiguration)
		{
			RegisterLogging(loggerConfiguration);

			if (distributedConfiguration == null)
			{
				// use nomad specific installer for that
				_windsorContainer.Install(
					new NomadEventAggregatorInstaller(),
					new NomadServiceLocatorInstaller(),
					new ModuleLoaderInstaller()
					);
			}
			else
			{
				// use nomad specific installer for that
				_windsorContainer.Install(
					new NomadDistributedDeliverySubsystemsInstaller(distributedConfiguration),
					new NomadDistributedEventAggregatorInstaller(),
					new NomadServiceLocatorInstaller(),
					new ModuleLoaderInstaller()
					);

				// TODO: make registering resolver with container later
				var dea =
					(DistributedEventAggregator)
					_windsorContainer.Resolve<IEventAggregator>(NomadDistributedEventAggregatorInstaller.ON_SITE_NAME);

				_resolver = new ResolverFactory(distributedConfiguration);
				dea.RemoteDistributedEventAggregator = _resolver.Resolve();

				// run service
				RegisterServiceHostDEA(distributedConfiguration);
			}
		}

		private void RegisterLogging(string loggerConfiguration)
		{
			var helper = new LoggingHelper();
			helper.RegisterLogging(loggerConfiguration, typeof (ContainerCreator));
			_logger = helper.Logger;
			_repository = helper.Repository;
		}
	}
}