﻿using System;

namespace Nomad.Communication.EventAggregation
{
    ///<summary>
    /// Contains necessary data about subscription ticket, it's status and all other necessary information
    ///</summary>
    /// <remarks>
    /// Instance of ticket is required to unsubscribe from <see cref="IEventAggregator"/>
    /// </remarks>
    ///<typeparam name="T">type of event you subscribed to</typeparam>
    public interface IEventAggregatorTicket<T> : IEventAggregatorTicket
    {
        ///<summary>
        /// Action which will be invoked, if the ticket is active
        ///</summary>
        //Action<T> Action { get; }
        /// <summary>
        /// Thread to deliver action in
        /// </summary>
        DeliveryMethod DeliveryMethod { get; }


        /// <summary>
        /// Executes ticket
        /// </summary>
        void Execute(T payload);
    }

    ///<summary>
    /// Event aggregator ticket is responsible for delivering payload to listener.
    ///</summary>
    /// <remarks>
    /// Ticket has to be disposable - it is the only way to remove it from EventAggregator
    /// </remarks>
    public interface IEventAggregatorTicket : IDisposable
    {
        ///<summary>
        /// Type of ticket message which ticket is interested in
        ///</summary>
        Type ActionType { get; }


        ///<summary>
        /// Executes action of the ticket passing payload.
        ///</summary>
        /// <remarks>
        /// Payload has to be proper type. Otherwise <see cref="ArgumentException"/> will be thrown
        /// </remarks>
        ///<param name="payload">payload to pass to listeners</param>
        /// <exception cref="ArgumentException">when payload is not the proper type</exception>
        void Execute(object payload);


        ///<summary>
        /// Invoked when ticket is disposed
        ///</summary>
        event EventHandler<TicketDisposedArgs> TicketDisposed;
    }
}