using System;
using System.IO;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Modules.Discovery;
using Nomad.Tests.Data.Distributed.Topic;
using Nomad.Utils.ManifestCreator;
using NUnit.Framework;
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
			// we are using the elements from this namespace
			SetSourceFolder(typeof (DistributableMessage));

			// TODO: this code is not refactor aware
			string sharedModuleSourcePath = GetSourceCodePath(@"DistributableMessage.cs");
			string publisherModuleSourcePath = GetSourceCodePath(@"SimplePublishingModule.cs");
			string listenerModuleSourcePath = GetSourceCodePath(@"SimpleListeningModule.cs");

			const string sharedPath = @"Modules\Distributed\Shared\";
			const string publisherPath = @"Modules\Distributed\Publisher\";
			const string listenerPath = @"Modules\Distributed\Listener\";

			// shared module generation
			Compiler.OutputDirectory = sharedPath;
			Compiler.GenerateModuleFromCode(sharedModuleSourcePath);


			// listener module generation
			Compiler.OutputDirectory = listenerPath;
			File.Copy(sharedPath + "DistributableMessage.dll", Path.Combine(listenerPath, "DistributableMessage.dll"), true);
			Compiler.GenerateModuleFromCode(listenerModuleSourcePath, sharedPath + "DistributableMessage.dll");
			var builder = new ManifestBuilder(@"TEST_ISSUER",
											  KeyFile,
			                                  @"SimpleListeningModule.dll",
			                                  listenerPath);
			builder.CreateAndPublish();

			// publisher module generation
			Compiler.OutputDirectory = publisherPath;
			File.Copy(sharedPath + "DistributableMessage.dll", Path.Combine(publisherPath, "DistributableMessage.dll"), true);
			Compiler.GenerateModuleFromCode(publisherModuleSourcePath, sharedPath + "DistributableMessage.dll");
			builder = new ManifestBuilder(@"TEST_ISSUER",
			                              KeyFile,
			                              @"SimplePublishingModule.dll",
			                              publisherPath);
			builder.CreateAndPublish();

			// creating listener module kernel
			string site1 = "net.tcp://127.0.0.1:5555/IDEA";
			NomadConfiguration config = NomadConfiguration.Default;
			config.DistributedConfiguration = DistributedConfiguration.Default;
			config.DistributedConfiguration.LocalURI = new Uri(site1);
			ListenerKernel = new NomadKernel(config);
			var listenerDiscovery = new DirectoryModuleDiscovery(listenerPath, SearchOption.TopDirectoryOnly);
			ListenerKernel.LoadModules(listenerDiscovery);

			// creating publisher module kernel
			string site2 = "net.tcp://127.0.0.1:6666/IDEA";
			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(site2);
			config2.DistributedConfiguration.URLs.Add(site1);
			PublisherKernel = new NomadKernel(config2);
			var publisherDiscovery = new DirectoryModuleDiscovery(publisherPath, SearchOption.TopDirectoryOnly);
			PublisherKernel.LoadModules(publisherDiscovery);

			var fi = new FileInfo(@"Modules\Distributed\Listener\CounterFile");
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
		[Ignore("Not yet implemented")]
		public void one_module_publishes_two_module_listens()
		{
			
		}
	}
}