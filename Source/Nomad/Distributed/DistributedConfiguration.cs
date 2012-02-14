using System;
using System.Collections.Generic;
using Nomad.Communication.EventAggregation;
using Nomad.Distributed.Communication;
using Nomad.Distributed.Communication.Deliveries.SingleDelivery;
using Nomad.Distributed.Communication.Deliveries.TimedDelivery;
using Nomad.Distributed.Communication.Deliveries.TopicDelivery;

namespace Nomad.Distributed
{
	/// <summary>
	///     Describes the configuration of the distributed part of the system. This configuration
	/// is the only configuration that is needed to use distribted services in <see cref="Nomad"/>.
	/// 
	/// <para>
	///     This is simplified version of the standard WCF configuration, because the neeed of
	/// providing whole WCF xml based configuration is to tiresome.
	/// </para>
	/// TODO: provide ability to freeze such configuration the same as NomadConfiguration
	/// </summary>
	[Serializable]
	public class DistributedConfiguration
	{
		/// <summary>
		///     Hidden constructor
		/// </summary>
		private DistributedConfiguration()
		{
		}

		/// <summary>
		///     Gets default configuration of the distributed enviorment
		/// </summary>
		public static DistributedConfiguration Default
		{
			get
			{
				return new DistributedConfiguration
				       	{
				       		LocalURI = new Uri("net.tcp://127.0.0.1:5555/IDEA"),
				       		URLs = new List<string>(),
				       		Mode = ResolutionMode.Manual,
				       		TopicDeliverySubsystem = new BasicTopicDeliverySubsystem(),
				       		SingleDeliverySubsystem = new BasicSingleDeliverySubsystem(),
				       		TimedDeliverySubsystem = new BasicTimedDeliverySubsystem()
				       	};
			}
		}

		public ResolutionMode Mode { get; set; }

		/// <summary>
		///     List of URLs where the other 
		/// </summary>
		public IList<string> URLs { get; set; }

		/// <summary>
		///     The URI at which the service of this <see cref="DistributedEventAggregator"/> has to
		/// be served.
		/// </summary>
		public Uri LocalURI { get; set; }

		/// <summary>
		///		Describes the used subsystem for topic deliveries aka <see cref="IEventAggregator.Publish{T}"/> method.
		/// </summary>
		/// <remarks>
		///		The provided implementations must be <see cref="SerializableAttribute"/> marked.
		/// </remarks>
		public ITopicDeliverySubsystem TopicDeliverySubsystem { get; set; }

		/// <summary>
		///		Describes the used subsystem for sinlge deliveeries aka <see cref="IEventAggregator.PublishSingleDelivery{T}"/> method.
		/// </summary>
		/// <remarks>
		///		The provided implementations must be <see cref="SerializableAttribute"/> marked.
		/// </remarks>
		public ISingleDeliverySubsystem SingleDeliverySubsystem { get; set; }

		/// <summary>
		///		Describes the used subsystem for timed delivery aka <see cref="IEventAggregator.PublishTimelyBuffered{T}"/> method.
		/// </summary>
		/// <remarks>
		///		The provided implementations must be <see cref="SerializableAttribute"/> marked.
		/// </remarks>
		public ITimedDeliverySubsystem TimedDeliverySubsystem { get; set; }
	}
}