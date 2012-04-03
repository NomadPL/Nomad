using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
			get
			{
				// get the type of 
                Assembly asm = AppDomain.CurrentDomain.GetAssemblies().Single(x => x.GetName().Name.Equals("DistributableMessage"));
				Type type = asm.GetType("Nomad.Tests.Data.Distributed.Commons.DistributedMessageRegistry");

				var methodInfo = type.GetProperty("Messages");
				var result = methodInfo.GetValue(null, null);

				return (IList<string>) result;

			}
		}
	}
}