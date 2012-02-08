using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Nomad.Messages.Distributed
{
	[DataContract]
	[Serializable]
	public class NomadSimpleMessage : NomadMessage
	{

		public NomadSimpleMessage(string message) : base(message)
		{
		}

		public override string ToString()
		{
			return Message;
		}
	}
}
