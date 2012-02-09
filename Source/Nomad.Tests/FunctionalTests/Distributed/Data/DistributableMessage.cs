namespace Nomad.Tests.FunctionalTests.Distributed.Data
{
	/// <summary>
	///		Sample class with payload data inside
	/// </summary>
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