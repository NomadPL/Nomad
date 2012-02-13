using System;
using System.IO;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Modules.Discovery;
using Nomad.Tests.Data.Distributed.Commons;
using Nomad.Tests.Data.Distributed.Topic;
using Nomad.Utils.ManifestCreator;
using NUnit.Framework;
using Nomad.Utils.ManifestCreator.DependenciesProvider;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	/// <summary>
	///		Tests the <see cref="Nomad.Distributed"/> mechanisms at the functional level of testing.
	/// </summary>
	[FunctionalTests]
	public class TopicDistributedNomad : DistributedNomadBase
	{
		[Test]
		public void module_publishes_module_listens()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof (SimplePublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof (SimpleListeningModule));

			string listener1 = GenerateListener(_runtimePath, _sharedDll, listeningModuleSrc, 1);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, _sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			// create listener site
			string listenerSite = "net.tcp://127.0.0.1:5555/IDEA";

			// create published sites
			string publisherSite = "net.tcp://127.0.0.1:7777/IDEA";

			// create kernels with configuration
			NomadConfiguration config1 = NomadConfiguration.Default;
			config1.DistributedConfiguration = DistributedConfiguration.Default;
			config1.DistributedConfiguration.LocalURI = new Uri(listenerSite);
			ListenerKernel = new NomadKernel(config1);
			IModuleDiscovery listenerDiscovery = new SingleModuleDiscovery(listener1);
			ListenerKernel.LoadModules(listenerDiscovery);

			// create publishing kernel
			NomadConfiguration publisherConfig = NomadConfiguration.Default;
			publisherConfig.DistributedConfiguration = DistributedConfiguration.Default;
			publisherConfig.DistributedConfiguration.LocalURI = new Uri(publisherSite);
			publisherConfig.DistributedConfiguration.URLs.Add(listenerSite);
			PublisherKernel = new NomadKernel(publisherConfig);
			IModuleDiscovery publisherDiscovery = new SingleModuleDiscovery(publisherDll);
			PublisherKernel.LoadModules(publisherDiscovery);


			// assert the events being published	
			var fi = new FileInfo(listener1 + "_CounterFile");
			if (fi.Exists)
			{
				StreamReader counterReader = fi.OpenText();
				int value = Convert.ToInt32(counterReader.ReadLine());
				// Verifying that locally the event aggregator works properly
				Assert.AreEqual(5, value);
				counterReader.Close();
			}
			else
			{
				Assert.Fail("No counter file from listener module in distributed configuration");
			}
		}

		[Test]
		public void one_module_publishes_two_module_listens()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof(SimplePublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof(SimpleListeningModule));

			string listener1 = GenerateListener(_runtimePath, _sharedDll, listeningModuleSrc, 1);
			string listener2 = GenerateListener(_runtimePath, _sharedDll, listeningModuleSrc, 2);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, _sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			// create listener site
			string listener1Site = "net.tcp://127.0.0.1:5555/IDEA";
			string listener2Site = "net.tcp://127.0.0.1:6666/IDEA";

			// create published sites
			string publisherSite = "net.tcp://127.0.0.1:7777/IDEA";

			// create listener kernels with configuration
			NomadConfiguration config1 = NomadConfiguration.Default;
			config1.DistributedConfiguration = DistributedConfiguration.Default;
			config1.DistributedConfiguration.LocalURI = new Uri(listener1Site);
			ListenerKernel = new NomadKernel(config1);
			IModuleDiscovery listenerDiscovery = new SingleModuleDiscovery(listener1);
			ListenerKernel.LoadModules(listenerDiscovery);

			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(listener2Site);
			ListenerKernel = new NomadKernel(config2);
			IModuleDiscovery listener2Discovery = new SingleModuleDiscovery(listener2);
			ListenerKernel.LoadModules(listener2Discovery);

			// create publishing kernel
			NomadConfiguration publisherConfig = NomadConfiguration.Default;
			publisherConfig.DistributedConfiguration = DistributedConfiguration.Default;
			publisherConfig.DistributedConfiguration.LocalURI = new Uri(publisherSite);
			publisherConfig.DistributedConfiguration.URLs.Add(listener1Site);
			publisherConfig.DistributedConfiguration.URLs.Add(listener2Site);
			PublisherKernel = new NomadKernel(publisherConfig);
			IModuleDiscovery publisherDiscovery = new SingleModuleDiscovery(publisherDll);
			PublisherKernel.LoadModules(publisherDiscovery);


			// assert the events being published	
			// listener1
			var fi = new FileInfo(listener1 + "_CounterFile");
			if (fi.Exists)
			{
				StreamReader counterReader = fi.OpenText();
				int value = Convert.ToInt32(counterReader.ReadLine());
				// Verifying that locally the event aggregator works properly
				Assert.AreEqual(5, value);
				counterReader.Close();
			}
			else
			{
				Assert.Fail("No counter file from listener module in distributed configuration");
			}

			// listener2
			fi = new FileInfo(listener2 + "_CounterFile");
			if (fi.Exists)
			{
				StreamReader counterReader = fi.OpenText();
				int value = Convert.ToInt32(counterReader.ReadLine());
				// Verifying that locally the event aggregator works properly
				Assert.AreEqual(5, value);
				counterReader.Close();
			}
			else
			{
				Assert.Fail("No counter file from listener module in distributed configuration");
			}	
		}
	}
}