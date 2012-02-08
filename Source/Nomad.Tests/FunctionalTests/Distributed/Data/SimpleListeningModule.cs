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

		public SimpleListeningModule(IEventAggregator eventAggregator)
		{
			_eventAggregator = eventAggregator;
		}

		public void OnLoad()
		{
			var ticket = _eventAggregator.Subscribe<SharedInterfaces>(CallBack);
		}

		private void CallBack(SharedInterfaces obj)
		{
			Console.WriteLine("Recieved: {0}", obj.Payload);
		}

		public void OnUnLoad()
		{
			// do nothing
		}
	}
}