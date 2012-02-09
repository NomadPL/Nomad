using System;
using System.Runtime.Serialization;
using Nomad.Messages;

namespace Nomad.Tests.FunctionalTests.Distributed
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
