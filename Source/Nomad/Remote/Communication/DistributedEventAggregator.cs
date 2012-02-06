using System;
using Nomad.Communication.EventAggregation;

namespace Nomad.Remote.Communication
{
    /// <summary>
    ///     Distributed version of <see cref="IEventAggregator"/> which enables Nomad application
    /// communicate over WCF links.
    /// </summary>
    /// <remarks>
    ///     This class is visible to the modules loaded to Nomad as simple <see cref="IEventAggregator"/>. 
    /// </remarks>
    public class DistributedEventAggregator : IEventAggregator
    {
        public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action) where T : class
        {
            throw new NotImplementedException();
        }

        public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action, DeliveryMethod deliveryMethod) where T : class
        {
            throw new NotImplementedException();
        }

        public void Publish<T>(T message) where T : class
        {
            throw new NotImplementedException();
        }
    }
}