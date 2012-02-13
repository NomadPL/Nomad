using System;
using System.IO;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Modules.Discovery;
using Nomad.Tests.Data.Distributed.TimeBuffered;
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
	public class TimelyBufferedDelivery : DistributedNomadBase
	{
		[Test]
		public void local_module_publishes_and_later_loaded_listener_module_receives_those_messages()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof (BufferedPublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof (SimpleListeningModule));

			string listener1 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 1);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, _sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			// preaparing modules discoveries.
			IModuleDiscovery listenerDiscovery = new SingleModuleDiscovery(listener1);
			IModuleDiscovery publisherDiscovery = new SingleModuleDiscovery(publisherDll);

			// create non-distributed kernel
			PublisherKernel = new NomadKernel();

			// publisher module load
			PublisherKernel.LoadModules(publisherDiscovery);

			// postponed listener module load
			PublisherKernel.LoadModules(listenerDiscovery);

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
				Assert.Fail("No counter file from listener module in local postponed configuration");
			}
		}


		[Test]
		public void local_module_publishes_and_later_loaded_listener_module_does_not_receive_outdated_messages()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof (InThePastPublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof (SimpleListeningModule));

			string listener1 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 1);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, _sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			// preaparing modules discoveries.
			IModuleDiscovery listenerDiscovery = new SingleModuleDiscovery(listener1);
			IModuleDiscovery publisherDiscovery = new SingleModuleDiscovery(publisherDll);

			// create non-distributed kernel
			PublisherKernel = new NomadKernel();

			// publisher module load
			PublisherKernel.LoadModules(publisherDiscovery);

			// postponed listener module load
			PublisherKernel.LoadModules(listenerDiscovery);

			// assert the events being published	
			var fi = new FileInfo(listener1 + "_CounterFile");
			Assert.False(fi.Exists);
		}

		[Test]
		public void local_module_publishes_5_outdated_and_5_valid_messages_and_later_loaded_listener_module_receives_those_messages()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof(MixedBufferedPublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof(SimpleListeningModule));

			string listener1 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 1);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, _sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			// preaparing modules discoveries.
			IModuleDiscovery listenerDiscovery = new SingleModuleDiscovery(listener1);
			IModuleDiscovery publisherDiscovery = new SingleModuleDiscovery(publisherDll);

			// create non-distributed kernel
			PublisherKernel = new NomadKernel();

			// publisher module load
			PublisherKernel.LoadModules(publisherDiscovery);

			// postponed listener module load
			PublisherKernel.LoadModules(listenerDiscovery);

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
				Assert.Fail("No counter file from listener module in local postponed configuration");
			}
		}

		[Test]
		public void distributed_module_publishes_and_already_loaded_listener_module_receives_those_messages()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof (BufferedPublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof (SimpleListeningModule));

			string listener1 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 1);

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
				Assert.Fail("No counter file from listener module in local postponed configuration");
			}
		}

		[Test]
		public void distributed_module_publishes_and_two_later_loaded_listener_modules_receive_those_messages()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof(MixedBufferedPublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof(SimpleListeningModule));

			string listener1 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 1);
			string listener2 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 1);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, _sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			// create listener site
			string listenerSite = "net.tcp://127.0.0.1:5555/IDEA";

			// create published sites
			string publisherSite = "net.tcp://127.0.0.1:7777/IDEA";

			// create listener1 kernel
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

			// postponed load of a second listener module 
			IModuleDiscovery listenerDiscovery2 = new SingleModuleDiscovery(listener2);
			ListenerKernel.LoadModules(listenerDiscovery2);

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
				Assert.Fail("No counter file from listener module in local postponed configuration");
			}

			// assert that last loaded listener2 received valid events
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
				Assert.Fail("No counter file from listener module in local postponed configuration");
			}
		}
	}
}