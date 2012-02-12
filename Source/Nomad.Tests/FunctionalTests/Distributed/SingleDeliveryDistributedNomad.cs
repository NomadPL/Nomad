using System;
using System.IO;
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
		[Test]
		[Ignore("Not yet implemented")]
		public void kernel_published_once_only_one_module_revieved()
		{
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

			string sharedDll = Compiler.GenerateModuleFromCode(sharedModuleSrc);
			Compiler.GenerateManifestForModule(sharedDll, KeyFile);

			string publisherDll = Compiler.GenerateModuleFromCode(publishingModuleSrc, sharedDll);
			var manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider(sharedDll);
			Compiler.GenerateManifestForModule(publisherDll, KeyFile, manifestConfiguration);

			string listener1 = GenerateListener(runtimePath, sharedDll, listeningModuleSrc, 1);
			string listneer2 = GenerateListener(runtimePath, sharedDll, listeningModuleSrc, 2);


			// create kernels with configuration


			// load


			// get the single delivery checking
		}

		private string GenerateListener(string runtimePath, string sharedDll, string listeningModuleSrc, int counter)
		{
			Compiler.OutputName = Path.Combine(runtimePath, "listener" + counter + ".dll");
			string listenerDll = Compiler.GenerateModuleFromCode(listeningModuleSrc, sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider(sharedDll);
			Compiler.GenerateManifestForModule(listenerDll, KeyFile,manifestConfiguration);

			return listenerDll;
		}
	}
}