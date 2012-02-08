using Nomad.Communication.ServiceLocation;
using Nomad.Remote.Communication;

namespace Nomad.Remote
{
    /// <summary>
    ///     Desribes the type of how the distributed part can be configured
    /// </summary>
    public enum ResolutionMode
    {
        /// <summary>
        ///     The locations of the other <see cref="DistributedEventAggregator"/> instances 
        /// are passed as simple URL of the machines.
        /// </summary>
        Manual,

        /// <summary>
        ///     The locations of the <see cref="DistributedEventAggregator"/> instances 
        /// are passed externalny using some not precesied yet interface.
        /// </summary>
        /// <remarks>
        ///     TODO: precise this way of doings
        /// </remarks>
        External,

        /// <summary>
        ///     The location is resolved using <see cref="IServiceLocator"/> and 
        /// some other not precised yet resolution service.
        /// </summary>
        /// <remarks>
        ///     TODO: precise this way of doings
        /// </remarks>
        Service
    }
}