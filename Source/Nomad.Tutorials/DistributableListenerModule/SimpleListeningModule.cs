using System;
using System.IO;
using DistributableMessagesLibrary;
using Nomad.Communication.EventAggregation;
using Nomad.Modules;

namespace DistributableListenerModule
{
	/// <summary>
	///		Simple module for listening to data
	/// </summary>
	public class SimpleListeningModule : IModuleBootstraper
	{
		private readonly IEventAggregator _eventAggregator;
		private int _counter;

		public SimpleListeningModule(IEventAggregator eventAggregator)
		{
			_counter = 0;
			_eventAggregator = eventAggregator;
		}

		public void OnLoad()
		{
			_eventAggregator.Subscribe<DistributableMessage>(CallBack);
			Console.WriteLine("Listener subscribed to DistributableMessage");
		}

		private void CallBack(DistributableMessage obj)
		{
			++_counter;
			Console.WriteLine("Listener got message with content: {0}", obj.Payload);
			Console.WriteLine("Listener counter: {0}", _counter);
		}

		public void OnUnLoad()
		{
		}
	}
}