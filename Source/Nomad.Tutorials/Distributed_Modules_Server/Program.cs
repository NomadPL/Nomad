﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Modules.Discovery;

namespace Distributed_Modules_Server
{
	class Program
	{
		private static void Main()
		{
			// signing the assemblies and creating the manifest using manifestBuilder api
			GenerateManifestUsingApi("DistributableListenerModule.dll", @"..\Modules\Listener");



			// using default distributed configuration
			string site1 = "net.tcp://127.0.0.1:5555/IDEA";
			NomadConfiguration config = NomadConfiguration.Default;
			config.DistributedConfiguration = DistributedConfiguration.Default;
			config.DistributedConfiguration.LocalURI = new Uri(site1);
			var kernel = new NomadKernel(config);

			// loading modules using single module discovery pattern
			var discovery =
				new DirectoryModuleDiscovery(@"..\Modules\Listener", SearchOption.TopDirectoryOnly);
			kernel.LoadModules(discovery);

			var modules = kernel.GetLoadedModules();
			Console.WriteLine("Listener kernel ready");
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
