using System;
using System.Collections.Generic;
using System.Security.Policy;
using Nomad.Communication.EventAggregation;
using Nomad.Communication.ServiceLocation;
using Nomad.Exceptions;
using Nomad.Messages.Loading;
using Nomad.Modules;
using Nomad.Modules.Discovery;
using Nomad.Services;
using Nomad.Updater;

namespace Nomad.Core
{
	/// <summary>
	/// Nomad's entry point. Presents Nomad's features to the developer.
	/// </summary>
	public class NomadKernel : IModulesOperations, IDisposable
	{
		private ModuleManager _moduleManager;
		private ContainerCreator _moduleLoaderCreator;

		/// <summary>
		/// Initializes new instance of the <see cref="NomadKernel"/> class.
		/// </summary>
		/// <param name="nomadConfiguration">
		/// <see cref="NomadConfiguration"/> used to initialize kernel modules.
		/// </param>
		/// <remarks>
		///     <para>
		///         Initializes both LoadedModules AppDomain with IModuleLoader implementation.
		///     </para>
		///     <para>
		///         Kernel, by now, uses the Nomad's default implementation of IModuleLoader( <see cref="ModuleLoader"/> with no ability to change it. 
		///         This constraint is made on behalf of the dependency on the IoC container which should be used for storing information about loaded modules. 
		///     </para>
		/// </remarks>
		public NomadKernel(NomadConfiguration nomadConfiguration)
		{
			if (nomadConfiguration == null)
			{
				throw new ArgumentNullException("nomadConfiguration",
				                                "Configuration must be provided.");
			}
			nomadConfiguration.Freeze();
			KernelConfiguration = nomadConfiguration;

			KernelAppDomain = AppDomain.CurrentDomain;

			// create another app domain and register very important services
			RegisterCoreServices(nomadConfiguration);

			// registering additional services ie. updater + listing + languages... etc
			RegisterAdditionalServices();
		}


		/// <summary>
		/// Initializes new instance of the <see cref="NomadKernel"/> class.
		/// Uses frozen <see cref="NomadConfiguration.Default"/> as configuration data.
		/// </summary>
		/// <remarks>
		///     <para>
		///         Initializes both LoadedModules AppDomain with IModuleLoader implementation.
		///     </para>
		///     <para>
		///         Kernel, by now, uses the Nomad default implementation of IModuleLoader( <see cref="ModuleLoader"/> with no possibility to changing it. 
		///         This constraint is made because of the dependency on the IoC container which should be used for storing information about 
		///     </para>
		/// </remarks>
		public NomadKernel() : this(NomadConfiguration.Default)
		{
		}


		/// <summary>
		///     IModuleLoader used for loading the modules by <see cref="_moduleManager"/>. 
		/// </summary>
		/// <remarks>
		///     Instantiated within constructor in ModuleAppDomain.
		/// </remarks>
		private IModuleLoader ModuleLoader { get; set; }

		/// <summary>
		///     AppDomain handler (read only) for AppDomain used for storing all loaded modules.
		/// </summary>
		public AppDomain ModuleAppDomain { get; private set; }

		/// <summary>
		///     AppDomain handler (read only) for AppDomain representing appDomain for <see cref="NomadKernel"/> instance.
		/// </summary>
		public AppDomain KernelAppDomain { get; private set; }


		/// <summary>
		///     Provides read only access to initialized Kernel configuration.
		/// </summary>
		public NomadConfiguration KernelConfiguration { get; private set; }

		/// <summary>
		///     Provides read only access to <see cref="IEventAggregator"/> object. Allows asynchronous communication with modules.
		/// </summary>
		/// <remarks>
		///     Communication from kernel is much slower because of the marshalling mechanism on app domain boundary.
		/// </remarks>
		public IEventAggregator EventAggregator { get; private set; }

		/// <summary>
		///       Provides read only access to <see cref="IServiceLocator"/> object. Allows synchronous communication with modules.
		/// </summary>
		/// <remarks>
		///     Communication from kernel is much slower because of the marshalling mechanism on app domain boundary.
		/// </remarks>
		public IServiceLocator ServiceLocator { get; private set; }

		#region IDisposable Members

		public void Dispose()
		{
			if (_moduleLoaderCreator != null)
			{
				_moduleLoaderCreator.Dispose();
			}
		}

		#endregion

		#region IModulesOperations Members

		/// <summary>
		///     Unloads the whole ModuleAppDomain.
		/// </summary>
		/// <remarks>
		///     New AppDomain with the same evidence settings and entry point is set after unloading.
		/// </remarks>
		public void UnloadModules()
		{
			_moduleManager.InvokeUnloadCallback();

			// dispose all the elements on the other site 
			_moduleLoaderCreator.Dispose();

			AppDomain.Unload(ModuleAppDomain);
			ModuleAppDomain = null;

			// re register of the Nomad Core Services
			RegisterCoreServices(KernelConfiguration);

			// re register Nomad Additional Services
			RegisterAdditionalServices();
		}


		/// <summary>
		///     Loads modules into their domain.
		/// </summary>
		/// <param name="moduleDiscovery">ModuleDiscovery specifying modules to be loaded.</param>
		/// <remarks>
		///     This method provides feedback to already loaded modules about any possible failure.
		/// </remarks>
		/// <exception cref="NomadCouldNotLoadModuleException">
		///     This exception will be raised when <see cref="ModuleManager"/> object responsible for
		/// loading modules encounter any problems. Any exception will be changed to the message <see cref="NomadCouldNotLoadModuleMessage"/> responsible for 
		/// informing other modules about failure.
		/// </exception>
		public void LoadModules(IModuleDiscovery moduleDiscovery)
		{
			try
			{
				_moduleManager.LoadModules(moduleDiscovery);
				EventAggregator.Publish(
					new NomadAllModulesLoadedMessage(
						new List<ModuleInfo>(moduleDiscovery.GetModules()),
						"Modules loaded successfully."));
			}
			catch (NomadCouldNotLoadModuleException e)
			{
				// publish event about not loading module to other modules.
				EventAggregator.Publish(new NomadCouldNotLoadModuleMessage(
				                        	"Could not load modules", e.ModuleName));

				// rethrow this exception to kernel domain 
				throw;
			}
		}


		public IEnumerable<ModuleInfo> GetLoadedModules()
		{
			// delegate the getting this into service, instead of implementing 
			return ServiceLocator.Resolve<ILoadedModulesService>().GetLoadedModules();
		}

		#endregion

		private void RegisterCoreServices(NomadConfiguration nomadConfiguration)
		{
			ModuleAppDomain = AppDomain.CreateDomain("Modules AppDomain",
			                                         new Evidence(AppDomain.CurrentDomain.Evidence),
			                                         AppDomain.CurrentDomain.BaseDirectory,
			                                         AppDomain.CurrentDomain.BaseDirectory,
			                                         true);

			// create kernel version of the event aggregator4
			var siteEventAggregator = new EventAggregator(new NullGuiThreadProvider());

			// use container creator to create communication services on modules app domain
			string asmName = typeof (ContainerCreator).Assembly.FullName;
			string typeName = typeof (ContainerCreator).FullName;

			if (typeName != null)
			{
				_moduleLoaderCreator = (ContainerCreator)
				                       ModuleAppDomain.CreateInstanceAndUnwrap(asmName, typeName);

				var distributedConfiguration = nomadConfiguration.DistributedConfiguration;
				_moduleLoaderCreator.Install(distributedConfiguration);

				// create facade for event aggregator combining proxy and on site object
				EventAggregator = new ForwardingEventAggregator(_moduleLoaderCreator.EventAggregatorOnModulesDomain,
				                                                siteEventAggregator);

				// used proxied service locator
				ServiceLocator = _moduleLoaderCreator.ServiceLocator;

				ModuleLoader = _moduleLoaderCreator.CreateModuleLoaderInstance();
			}

			_moduleManager = new ModuleManager(ModuleLoader,
			                                   KernelConfiguration.ModuleFilter,
			                                   KernelConfiguration.DependencyChecker);
		}


		private void RegisterAdditionalServices()
		{
			// registering updater using data from configuration
			// TODO: maybe changing the event aggregator not to be passed via constructor
			var updater = new NomadUpdater(KernelConfiguration.ModuleDirectoryPath,
			                               KernelConfiguration.ModuleRepository,
			                               this,
			                               EventAggregator,
			                               KernelConfiguration.ModulePackager,
			                               KernelConfiguration.DependencyChecker,
			                               KernelConfiguration.ModuleFinder) {Mode = KernelConfiguration.UpdaterType};

			// FIXME this into construcot of the updater ?
			ServiceLocator.Register<IUpdater>(updater);

			// registering LoadedModulesService
			ServiceLocator.Register<ILoadedModulesService>(
				new LoadedModulesService(ModuleLoader));
		}
	}
}