using System;
using System.Threading;
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
		[SetUp]
		public void set_up()
		{
			_sharedDll = string.Empty;
		}

		[Test]
		public void local_module_published_once_one_module_revieved()
		{
			PrepareSharedLibrary();


		}

		[Test]
		public void module_published_once_only_one_module_recieved()
		{
			// path for this test (using the test method name) use in each code
			PrepareSharedLibrary();

			string publishingModuleSrc = GetSourceCodePath(typeof (SDPublishingModule));
			string listeningModuleSrc = GetSourceCodePath(typeof (SDListeningModule));

			string listener1 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 1);
			string listener2 = GenerateListener(RuntimePath, _sharedDll, listeningModuleSrc, 2);

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
			IModuleDiscovery listnerDiscovery = new SingleModuleDiscovery(listener1);
			ListenerKernel.LoadModules(listnerDiscovery);
			var firstCarrier = CreateCarrier(ListenerKernel);

			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(site2);
			config2.DistributedConfiguration.URLs.Add(site1);
			config2.DistributedConfiguration.URLs.Add(publisherSite);
			ListenerKernelSecond = new NomadKernel(config2);
			IModuleDiscovery listenerDiscovery2 = new SingleModuleDiscovery(listener2);
			ListenerKernelSecond.LoadModules(listenerDiscovery2);
			var secondCarrier = CreateCarrier(ListenerKernelSecond);

			// create publishing kernel
			NomadConfiguration publisherConfig = NomadConfiguration.Default;
			publisherConfig.DistributedConfiguration = DistributedConfiguration.Default;
			publisherConfig.DistributedConfiguration.LocalURI = new Uri(publisherSite);
			publisherConfig.DistributedConfiguration.URLs.Add(site1);
			publisherConfig.DistributedConfiguration.URLs.Add(site2);
			PublisherKernel = new NomadKernel(publisherConfig);
			IModuleDiscovery publisherDiscovery = new SingleModuleDiscovery(publisherDll);
			PublisherKernel.LoadModules(publisherDiscovery);

			// assert the events being published
			// wait for publishing messages etc
			Thread.Sleep(PUBLISH_TIMEOUT);
			int firstMsg = firstCarrier.GetStatus.Count;
			int secondMsg = secondCarrier.GetStatus.Count;

			Assert.AreEqual(5, firstMsg + secondMsg, "The number of delivered messages is not exactly 5");
		}
	}
}