using System;
using System.Runtime.Serialization;

namespace Nomad.Messages.Distributed
{
	/// <summary>
	///     Abstract class that defines the message passed between distributed kernels.
	/// </summary>
	/// <remarks>
	///     This class has to be serializable.
	/// </remarks>
	[Serializable]
	public abstract class NomadDistributedMessage
	{
		/// <summary>
		///     Text message. About cause of the message.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		///     Protected constructor used only for inheritance lineage.
		/// </summary>
		/// <param name="message">Message to me passed with <see cref="NomadMessage"/> object. </param>
		protected NomadDistributedMessage(string message)
		{
			Message = message;
		}


		/// <summary>
		///     Inherited <see cref="object.ToString"/> method which every message has to implement.
		/// </summary>
		public abstract override string ToString();
	}
}