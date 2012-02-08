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

	}
}
