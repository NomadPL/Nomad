using System;
using System.IO;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Modules.Discovery;
using Nomad.Tests.Data.Distributed.SingleDelivery;
using Nomad.Utils.ManifestCreator;
using Nomad.Utils.ManifestCreator.DependenciesProvider;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	[FunctionalTests]
	public class SingleDeliveryDistributedNomad : DistributedNomadBase
	{
		private string _sharedDll;

		[SetUp]
		public void set_up()
		{
			_sharedDll = string.Empty;
		}

		[Test]
		public void kernel_published_once_only_one_module_revieved()
		{
			string listeningModuleSrc = GetSourceCodePath(typeof (SimpleListeningModule));
			string sharedModuleSrc = GetSourceCodePath(typeof (DistributableMessage));
		}

		[Test]
		public void module_published_once_only_one_module_recieved()
		{
			string publishingModuleSrc = GetSourceCodePath(typeof (SimplePublishingModule));

			string listeningModuleSrc = GetSourceCodePath(typeof (SimpleListeningModule));
			string sharedModuleSrc = GetSourceCodePath(typeof (DistributableMessage));

			// path for this test (using the test method name)
			string runtimePath = @"Modules\Distributed\" + GetCurrentMethodName();

			Compiler.OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, runtimePath);

			_sharedDll = Compiler.GenerateModuleFromCode(sharedModuleSrc);

			string listener1 = GenerateListener(runtimePath, _sharedDll, listeningModuleSrc, 1);
			string listener2 = GenerateListener(runtimePath, _sharedDll, listeningModuleSrc, 2);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, _sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			// create listeners sites
			string site1 = "net.tcp://127.0.0.1:5555/IDEA";
			string site2 = "net.tcp://127.0.0.1:6666/IDEA";

			// create published sites
			string publisherSite = "net.tcp://127.0.0.1:7777/IDEA";

			// create kernels with configuration
			NomadConfiguration config1 = NomadConfiguration.Default;
			config1.DistributedConfiguration = DistributedConfiguration.Default;
			config1.DistributedConfiguration.LocalURI = new Uri(site1);
			config1.DistributedConfiguration.URLs.Add(site2);
			config1.DistributedConfiguration.URLs.Add(publisherSite);
			ListenerKernel = new NomadKernel(config1);
			IModuleDiscovery listnerDiscovery = GetDiscovery(listener1);
			ListenerKernel.LoadModules(listnerDiscovery);

			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(site2);
			config2.DistributedConfiguration.URLs.Add(site1);
			config2.DistributedConfiguration.URLs.Add(publisherSite);
			ListenerKernelSecond = new NomadKernel(config2);
			IModuleDiscovery listenerDiscovery2 = GetDiscovery(listener2);
			ListenerKernelSecond.LoadModules(listenerDiscovery2);

			// create publishing kernel
			NomadConfiguration publisherConfig = NomadConfiguration.Default;
			publisherConfig.DistributedConfiguration = DistributedConfiguration.Default;
			publisherConfig.DistributedConfiguration.LocalURI = new Uri(publisherSite);
			publisherConfig.DistributedConfiguration.URLs.Add(site1);
			publisherConfig.DistributedConfiguration.URLs.Add(site2);
			PublisherKernel = new NomadKernel(publisherConfig);
			IModuleDiscovery publisherDiscovery = GetDiscovery(publisherDll);
			PublisherKernel.LoadModules(publisherDiscovery);

			// load


			// get the single delivery checking
		}

		private IModuleDiscovery GetDiscovery(string pathToDll)
		{
			return new CompositeModuleDiscovery(new SingleModuleDiscovery(pathToDll));
		}

		private string GenerateListener(string runtimePath, string sharedDll, string listeningModuleSrc, int counter)
		{
			Compiler.OutputName = Path.Combine(runtimePath, "listener" + counter + ".dll");
			string listenerDll = Compiler.GenerateModuleFromCode(listeningModuleSrc, sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(listenerDll, KeyFile, manifestConfiguration);

			return listenerDll;
		}
	}
}