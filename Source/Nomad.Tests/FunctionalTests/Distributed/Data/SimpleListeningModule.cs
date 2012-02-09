using System;
using Nomad.Communication.EventAggregation;
using Nomad.Modules;

namespace Nomad.Tests.FunctionalTests.Distributed.Data
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
			var ticket = _eventAggregator.Subscribe<DistributableMessage>(CallBack);
		}

		private void CallBack(DistributableMessage obj)
		{
			Console.WriteLine("Recieved: {0}", obj.Payload);
			++_counter;
		}

		public void OnUnLoad()
		{
			// do nothing
		}
	}
}