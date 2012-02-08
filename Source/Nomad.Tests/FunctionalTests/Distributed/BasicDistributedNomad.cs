using System;
using Nomad.Core;
using Nomad.Distributed;
using Nomad.Messages.Distributed;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	/// <summary>
	///		Tests <see cref="Nomad.Distributed"/> mechanisms at functional
	/// level. Simple beacause no compilaton at runtime is done.
	/// </summary>
	[FunctionalTests]
	internal class BasicDistributedNomad
	{
		private NomadKernel _kernel1;
		private NomadKernel _kernel2;

		[TearDown]
		public void tear_down()
		{
			if (_kernel1 != null)
			{
				_kernel1.Dispose();
			}

			if (_kernel2 != null)
			{
				_kernel2.Dispose();
			}
		}

		[Test]
		public void default_distributed_configuration_passes()
		{
			NomadConfiguration config = NomadConfiguration.Default;
			config.DistributedConfiguration = DistributedConfiguration.Default;
			_kernel1 = new NomadKernel(config);
			Assert.IsNotNull(_kernel1);
		}

		[Test]
		public void two_service_hosts_work_simultanously_on_different_ports()
		{
			string site1 = "net.tcp://127.0.0.1:5555/IDEA";
			string site2 = "net.tcp://127.0.0.1:6666/IDEA";
			NomadConfiguration config = NomadConfiguration.Default;
			config.DistributedConfiguration = DistributedConfiguration.Default;
			config.DistributedConfiguration.LocalURI = new Uri(site1);
			_kernel1 = new NomadKernel(config);

			NomadConfiguration config2 = NomadConfiguration.Default;
			config2.DistributedConfiguration = DistributedConfiguration.Default;
			config2.DistributedConfiguration.LocalURI = new Uri(site2);
			config2.DistributedConfiguration.URLs.Add(site1);
			_kernel2 = new NomadKernel(config2);

			Assert.IsNotNull(_kernel1);
			Assert.IsNotNull(_kernel2);

			_kernel2.EventAggregator.Publish(new NomadSimpleMessage("Hello from kernel2"));
		}
	}
}