using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Nomad.Remote.Communication;

namespace Nomad.Remote
{
	/// <summary>
	///     Describes the configuration of the distributed part of the system. This configuration
	/// is the only configuration that is needed to use distribted services in <see cref="Nomad"/>.
	/// 
	/// <para>
	///     This is simplified version of the standard WCF configuration, because the neeed of
	/// providing whole WCF xml based configuration is to tiresome.
	/// </para>
	/// </summary>
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
				return new DistributedConfiguration()
						   {
							   LocalURI = new Uri("net.tcp://127.0.0.1:5555/IDEA"),
							   LocalBinding = new NetTcpBinding()
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
		///     Binding used in the mode
		/// </summary>
		public Binding LocalBinding { get; set; }
	}
}