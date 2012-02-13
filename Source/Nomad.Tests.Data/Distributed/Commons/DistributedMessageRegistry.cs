using System.Collections.Generic;

namespace Nomad.Tests.Data.Distributed.Commons
{
	/// <summary>
	///		Helper class used for storing the information 
	/// for
	/// </summary>
	public static class DistributedMessageRegistry
	{
		private static volatile List<string> messagesInner = new List<string>();

		/// <summary>
		///		Gets the read only collection of already added messages.
		/// </summary>
		public static IList<string> Messages
		{
			get { return messagesInner.AsReadOnly(); }
		}

		/// <summary>
		///		Only adding messags to <see cref="DistributedMessageRegistry"/> can be done.
		/// </summary>
		/// <param name="msg"></param>
		public static void Add(string msg)
		{
			messagesInner.Add(msg);
		}
	}
}