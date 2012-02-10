using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Modules.Discovery;

namespace Client_Application
{
	class Program
	{
		private static void Main()
		{
			// signing the assemblies and creating the manifest using manifestBuilder api
			GenerateManifestUsingApi("DistributablePublisherModule.dll", @"..\Modules\Publisher");
			


			// using default configuration
			string publisherSite = "net.tcp://127.0.0.1:6666/IDEA";
			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(publisherSite);

			// adding listener address to known sites.
			const string listenerSite = "net.tcp://127.0.0.1:5555/IDEA";
			config2.DistributedConfiguration.URLs.Add(listenerSite);
			var kernel = new NomadKernel(config2);

			// loading modules using single module discovery pattern
			var discovery =
				new DirectoryModuleDiscovery(@"..\Modules\Publisher", SearchOption.TopDirectoryOnly);
			kernel.LoadModules(discovery);

			var publisherModules = kernel.GetLoadedModules();
			Console.WriteLine("Publisher kernel ready");
			//wait for input
			Console.ReadLine();
		}




		private static void GenerateManifestUsingApi(string assemblyName, string path)
		{
			var builder = new Nomad.Utils.ManifestCreator.ManifestBuilder(@"TUTORIAL_ISSUER",
																		  @"..\..\..\KEY_FILE.xml",
																		  assemblyName,
																		  path);
			builder.CreateAndPublish();
		}
	}
}
