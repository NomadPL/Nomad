using System;
using System.IO;
using System.Threading;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.KeysGenerator;
using Nomad.Messages.Loading;
using Nomad.Modules.Discovery;
using Nomad.Tests.FunctionalTests.Fixtures;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	/// <summary>
	///		Tests the <see cref="Nomad.Distributed"/> mechanisms at the functional level of testing.
	/// </summary>
	[FunctionalTests]
	public class TopicDistributedNomad
	{
		private readonly ModuleCompiler _compiler = new ModuleCompiler();
		private const string SOURDE_DIR = @"..\Source\Nomad.Tests\FunctionalTests\Data\Distributed";
		private NomadKernel _listenerKernel;
		private NomadKernel _publisherKernel;

		[TearDown]
		public void tear_down()
		{
			if (_listenerKernel != null)
			{
				_listenerKernel.Dispose();
			}

			if (_publisherKernel != null)
			{
				_publisherKernel.Dispose();
			}
		}

		private static String GetSourceCodePath(String codeLocation)
		{
			var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SOURDE_DIR);
			return Path.Combine(dirPath, codeLocation);
		}


		[Test]
		public void module_publishes_module_listens()
		{
			// TODO: this code is not refactor aware
			string sharedModuleSourcePath = GetSourceCodePath(@"DistributableMessage.cs");
			string publisherModuleSourcePath = GetSourceCodePath(@"SimplePublishingModule.cs");
			string listenerModuleSourcePath = GetSourceCodePath(@"SimpleListeningModule.cs");

			const string sharedPath = @"Modules\Distributed\Shared\";
			const string publisherPath = @"Modules\Distributed\Publisher\";
			const string listenerPath = @"Modules\Distributed\Listener\";

			// shared module generation
			_compiler.OutputDirectory = sharedPath;
			_compiler.GenerateModuleFromCode(sharedModuleSourcePath);


			string keyFile = @"alaMaKota.xml";
			if (File.Exists(keyFile))
			{
				File.Delete(keyFile);
			}
			KeysGeneratorProgram.Main(new[] {keyFile});

			// listener module generation
			_compiler.OutputDirectory = listenerPath;
			File.Copy(sharedPath + "DistributableMessage.dll", Path.Combine(listenerPath, "DistributableMessage.dll"), true);
			_compiler.GenerateModuleFromCode(listenerModuleSourcePath, sharedPath + "DistributableMessage.dll");
			var builder = new Nomad.Utils.ManifestCreator.ManifestBuilder(@"TEST_ISSUER",
			                                                              keyFile,
			                                                              @"SimpleListeningModule.dll",
			                                                              listenerPath);
			builder.CreateAndPublish();

			// publisher module generation
			_compiler.OutputDirectory = publisherPath;
			File.Copy(sharedPath + "DistributableMessage.dll", Path.Combine(publisherPath, "DistributableMessage.dll"), true);
			_compiler.GenerateModuleFromCode(publisherModuleSourcePath, sharedPath + "DistributableMessage.dll");
			builder = new Nomad.Utils.ManifestCreator.ManifestBuilder(@"TEST_ISSUER",
			                                                          keyFile,
			                                                          @"SimplePublishingModule.dll",
			                                                          publisherPath);
			builder.CreateAndPublish();

			if (File.Exists(keyFile))
			{
				File.Delete(keyFile);
			}

			// creating listener module kernel
			string site1 = "net.tcp://127.0.0.1:5555/IDEA";
			NomadConfiguration config = NomadConfiguration.Default;
			config.DistributedConfiguration = DistributedConfiguration.Default;
			config.DistributedConfiguration.LocalURI = new Uri(site1);
			_listenerKernel = new NomadKernel(config);
			var listenerDiscovery = new DirectoryModuleDiscovery(listenerPath, SearchOption.TopDirectoryOnly);
			_listenerKernel.LoadModules(listenerDiscovery);
		
			// creating publisher module kernel
			string site2 = "net.tcp://127.0.0.1:6666/IDEA";
			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(site2);
			config2.DistributedConfiguration.URLs.Add(site1);
			_publisherKernel = new NomadKernel(config2);
			var publisherDiscovery = new DirectoryModuleDiscovery(publisherPath, SearchOption.TopDirectoryOnly);
			_publisherKernel.LoadModules(publisherDiscovery);

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
	}
}