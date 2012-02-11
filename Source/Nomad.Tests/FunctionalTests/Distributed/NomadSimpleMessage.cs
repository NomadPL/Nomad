using System;
using System.Runtime.Serialization;
using Nomad.Messages.Distributed;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	[Serializable]
	public class NomadSimpleMessage : NomadDistributedMessage
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
