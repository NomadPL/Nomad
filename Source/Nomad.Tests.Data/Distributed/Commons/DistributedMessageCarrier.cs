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
				Assembly asm = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals("DistributableMessage")).Select( x => x).Single();
				Type type = asm.GetType("Nomad.Tests.Data.Distributed.Commons.DistributedMessageRegistry");

				var methodInfo = type.GetMethods().Where(x => x.Name.Contains("Messages")).Single();
				var result = methodInfo.Invoke(null, null);

				return (IList<string>) result;

			}
		}
	}
}