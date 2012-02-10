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
		private FileInfo _fileInfo = new FileInfo(@"CounterFile");

		public SimpleListeningModule(IEventAggregator eventAggregator)
		{
			_counter = 0;
			_eventAggregator = eventAggregator;
			_fileInfo.Delete();
		}

		public void OnLoad()
		{
			_eventAggregator.Subscribe<DistributableMessage>(CallBack);
		}

		private void CallBack(DistributableMessage obj)
		{
			++_counter;
			Console.WriteLine("Listener got message with content: {0}",obj.Payload);
			if (_counter >= 5)
			{
				StreamWriter text = _fileInfo.CreateText();
				text.WriteLine(_counter);
				text.Close();
			}
		}

		public void OnUnLoad()
		{
		}
	}
}