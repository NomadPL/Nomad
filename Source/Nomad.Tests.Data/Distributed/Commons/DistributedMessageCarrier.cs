using System;
using System.Collections.Generic;

namespace Nomad.Tests.Data.Distributed.Commons
{
	/// <summary>
	///		Initialize this class for creating the carrier and reading the current 
	/// status of the <see cref="DistributedMessageRegistry"/>.
	/// </summary>
	public class DistributedMessageCarrier : MarshalByRefObject
	{
		public IList<string> GetStatus
		{
			get { return DistributedMessageRegistry.Messages; }
		}
	}
}