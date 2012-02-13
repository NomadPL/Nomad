using System;
using System.Collections.Generic;
using Nomad.Messages;

namespace Nomad.Communication.EventAggregation
{
	///<summary>
	/// Provides implementation for <see cref="IEventAggregator"/> based on delegates
	///</summary>
	public class EventAggregator : MarshalByRefObject, IEventAggregator
	{
		private readonly IDictionary<Type, HashSet<IEventAggregatorTicket>> _subscriptions =
			new Dictionary<Type, HashSet<IEventAggregatorTicket>>();

		private IGuiThreadProvider _guiThreadProvider;

		private IEventAggregatorTicket<WpfGuiChangedMessage> _ticket;

		private readonly IDictionary<Type, List<BufferedMessage>> _bufferedMessages =
			new Dictionary<Type, List<BufferedMessage>>();


		///<summary>
		/// Initializes <see cref="EventAggregator"/> with provided <see cref="guiThreadProvider"/>.
		///</summary>
		public EventAggregator(IGuiThreadProvider guiThreadProvider)
		{
			_guiThreadProvider = guiThreadProvider;
			_ticket = Subscribe<WpfGuiChangedMessage>(GuiThreadChanged);
		}


		private void GuiThreadChanged(WpfGuiChangedMessage wpfGuiChangedMessage)
		{
			_guiThreadProvider = wpfGuiChangedMessage.NewGuiThreadProvider;
			Unsubscribe(_ticket);
			_ticket = Subscribe<WpfGuiChangedMessage>(GuiThreadChangedInvalid);
		}


		private void GuiThreadChangedInvalid(object obj)
		{
			throw new InvalidOperationException("Cannot set wpf gui thread twice!");
		}


		public override object InitializeLifetimeService()
		{
			// do not GC this element
			return null;
		}

		#region Implementation of IEventAggregator

		/// <summary>
		/// Adds action for execution.
		/// <see cref="IEventAggregator.Subscribe{T}(System.Action{T})"/>
		/// </summary>
		/// <remarks>
		/// Will be executed in any thread
		/// </remarks>
		/// <typeparam name="T">type of event to subsribe for</typeparam>
		/// <param name="action">action delegate to fire when type T delivered</param>
		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action) where T : class
		{
			return Subscribe(action, DeliveryMethod.AnyThread);
		}


		/// <summary>
		/// Subscribes action for specific event type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		/// <param name="deliveryMethod"></param>
		/// <returns></returns>
		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action,
		                                              DeliveryMethod deliveryMethod) where T : class
		{
			Type type = typeof (T);
			var ticket = new EventAggregatorTicket<T>(action, deliveryMethod, _guiThreadProvider);
			HashSet<IEventAggregatorTicket> tickets = null;
			lock (_subscriptions)
			{
				if (!_subscriptions.TryGetValue(type, out tickets))
				{
					tickets = new HashSet<IEventAggregatorTicket>();
					_subscriptions[type] = tickets;
				}
			}

			lock (tickets)
			{
				tickets.Add(ticket);
			}

			ticket.TicketDisposed += TicketDisposed;

			// deliverying possible awaiting buffered messages of T type
			List<BufferedMessage> tTypeBuffer;
			lock (_bufferedMessages)
			{
				if (!_bufferedMessages.TryGetValue(type, out tTypeBuffer))
				{
					tTypeBuffer = new List<BufferedMessage>();
					_bufferedMessages[type] = tTypeBuffer;
				}
			}

			// delivering current messages
			foreach (var bufferedMessage in tTypeBuffer)
			{
				if (bufferedMessage.ExpiryTime > DateTime.Now)
				{
					Publish((T)bufferedMessage.Message);
				}
			}

			// clearing outdate messages from the buffer
			tTypeBuffer.RemoveAll(m => m.ExpiryTime <= DateTime.Now);

			return ticket;
		}


		/// <summary>
		/// Notifies event listeners. Thread safe.
		/// <see cref="IEventAggregator.Publish{T}"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		public bool Publish<T>(T message) where T : class
		{
			Type type = typeof (T);
			HashSet<IEventAggregatorTicket> tickets;
			lock (_subscriptions)
			{
				_subscriptions.TryGetValue(type, out tickets);
			}

			//prevention from throwing exception
			if (tickets == null)
				return false;

			List<IEventAggregatorTicket> ticketsList;
			lock (tickets)
			{
				ticketsList = new List<IEventAggregatorTicket>(tickets);
			}

			foreach (IEventAggregatorTicket ticket in ticketsList)
			{
				ticket.Execute(message);
			}
			return true;
		}

		public void PublishTimelyBuffered<T>(T message, DateTime validUntil) where T : class
		{
			// adding message to local buffer
			Type type = typeof (T);
			var newMessage = new BufferedMessage(message, validUntil);
			List<BufferedMessage> tTypeBuffer;
			lock (_bufferedMessages)
			{
				if (!_bufferedMessages.TryGetValue(type, out tTypeBuffer))
				{
					tTypeBuffer = new List<BufferedMessage>();
					_bufferedMessages[type] = tTypeBuffer;
				}
			}

			lock (tTypeBuffer)
			{
				tTypeBuffer.Add(newMessage);
			}

			// delivering current messages
			foreach (var bufferedMessage in tTypeBuffer)
			{
				if (bufferedMessage.ExpiryTime > DateTime.Now)
				{
					Publish((T) bufferedMessage.Message);
				}
			}

			// clearing outdate messages from the buffer
			tTypeBuffer.RemoveAll(m => m.ExpiryTime <= DateTime.Now);
		}

		public bool PublishSingleDelivery<T>(T message, SingleDeliverySemantic singleDeliverySemantic) where T : class
		{
			throw new NotImplementedException();
		}


		private void TicketDisposed(object sender, TicketDisposedArgs e)
		{
			var ticket = sender as IEventAggregatorTicket;
			ticket.TicketDisposed -= TicketDisposed;
			Unsubscribe(ticket);
		}


		//TODO: Unsubsribing new lambda won't work!
		/// <summary>
		/// Unsubsribes specified action. 
		/// Removes event from collection. Thread safe.
		/// <see cref="IEventAggregator.Unsubscribe{T}"/>
		/// </summary>
		/// <param name="ticket">ticket have to be <see cref="EventAggregatorTicket{T}"/></param>
		/// <exception cref="KeyNotFoundException">when unsubscribing from type which was no subsription ever</exception>
		/// <exception cref="MemberAccessException"></exception>
		private void Unsubscribe(IEventAggregatorTicket ticket)
		{
			Type type = ticket.ActionType;
			HashSet<IEventAggregatorTicket> tickets = null;
			lock (_subscriptions)
			{
				tickets = _subscriptions[type];
			}
			lock (tickets)
			{
				tickets.Remove(ticket);
			}
		}

		#endregion
	}
}