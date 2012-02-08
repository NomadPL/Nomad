using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	[FunctionalTests]
	class DistributedNomadEnvironment
	{
		[Test]
		public void default_distributed_configuration_passes()
		{
			var config = Nomad.Core.NomadConfiguration.Default;
			config.DistributedConfiguration = Nomad.Distributed.DistributedConfiguration.Default;
			var kernel = new Nomad.Core.NomadKernel(config);
			Assert.IsNotNull(kernel);
		}

		[Test]
		public void two_Nomad_service_hosts_work_simultanously_on_different_ports()
		{
			string site1 = "net.tcp://127.0.0.1:6666/IDEA";
			string site2 = "net.tcp://127.0.0.1:7777/IDEA";
			var config = Nomad.Core.NomadConfiguration.Default;
			config.DistributedConfiguration = Nomad.Distributed.DistributedConfiguration.Default;
			config.DistributedConfiguration.LocalURI = new Uri(site1);
			var kernel = new Nomad.Core.NomadKernel(config);
			
			var config2 = Nomad.Core.NomadConfiguration.Default;
			config2.DistributedConfiguration = Nomad.Distributed.DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(site2);
			config2.DistributedConfiguration.URLs.Add(site1);
			var kernel2 = new Core.NomadKernel(config2);

			Assert.IsNotNull(kernel);
			Assert.IsNotNull(kernel2);
			
			kernel2.EventAggregator.Publish(new Nomad.Messages.Distributed.NomadSimpleMessage("Hello from kernel2"));
		}

	}
}
