using System;
using System.Runtime.Serialization;

namespace Nomad.Messages.Distributed
{
	/// <summary>
	///     Abstract class that defines the message passed between distributed kernels.
	/// </summary>
	/// <remarks>
	///     <para>This class has to be serializable.</para>
	/// <para>
	///  All the sepcialization of the <see cref="NomadDetachingMessage"/>
	/// need to be specified using the <see cref="KnownTypeAttribute"/>.
	/// </para>
	/// </remarks>
	[Serializable]
	[KnownType(typeof (NomadDetachingMessage))]
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
		///     Protected constructor used only for inheritance lineage. No <see cref="Message"/>
		/// will be set.
		/// </summary>
		protected NomadDistributedMessage(): this(string.Empty)
		{
			
		}

		/// <summary>
		///     Inherited <see cref="object.ToString"/> method which every message has to implement.
		/// </summary>
		public abstract override string ToString();
	}
}