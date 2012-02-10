using System;

namespace DistributableMessagesLibrary
{
	/// <summary>
	///		Sample class with payload data inside
	/// </summary>
	[Serializable]
	public class DistributableMessage
	{
		private readonly string _payload;

		public DistributableMessage(string payload)
		{
			_payload = payload;
		}

		public string Payload
		{
			get { return _payload; }
		}
	}
}
