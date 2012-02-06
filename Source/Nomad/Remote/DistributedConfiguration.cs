using System.Collections.Generic;

namespace Nomad.Remote
{
	/// <summary>
	///     Describes the configuration of the distributed part of the system.
	/// </summary>
	public class DistributedConfiguration
	{
		public IList<string> URLs { get; set; }
	}
}