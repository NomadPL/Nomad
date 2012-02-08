using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed_Modules_Server
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = Nomad.Core.NomadConfiguration.Default;
			config.DistributedConfiguration = Nomad.Distributed.DistributedConfiguration.Default;
			var kernel = new Nomad.Core.NomadKernel(config);
		}
	}
}
