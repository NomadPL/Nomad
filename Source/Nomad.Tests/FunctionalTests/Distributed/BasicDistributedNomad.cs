using System;
using Nomad.Core;
using Nomad.Distributed;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	/// <summary>
	///		Tests <see cref="Nomad.Distributed"/> mechanisms at functional
	/// level. Simple beacause no compilaton at runtime is done.
	/// </summary>
	[FunctionalTests]
	internal class BasicDistributedNomad : DistributedNomadBase
	{
		[Test]
		public void default_distributed_configuration_passes()
		{
			NomadConfiguration config = NomadConfiguration.Default;
			config.DistributedConfiguration = DistributedConfiguration.Default;
			Assert.DoesNotThrow(() => ListenerKernel = new NomadKernel(config));
		}

		[Test]
		public void two_service_hosts_work_simultanously_on_different_ports()
		{
			string site1 = "net.tcp://127.0.0.1:5555/IDEA";
			string site2 = "net.tcp://127.0.0.1:6666/IDEA";
			NomadConfiguration config = NomadConfiguration.Default;
			config.DistributedConfiguration = DistributedConfiguration.Default;
			config.DistributedConfiguration.LocalURI = new Uri(site1);
			Assert.DoesNotThrow(() => ListenerKernel = new NomadKernel(config));

			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(site2);
			config2.DistributedConfiguration.URLs.Add(site1);
			Assert.DoesNotThrow(() => PublisherKernel = new NomadKernel(config2));

			PublisherKernel.EventAggregator.Publish(new NomadSimpleMessage("Hello from kernel2"));

			Assert.DoesNotThrow(() => PublisherKernel.Dispose());
		}
	}
}