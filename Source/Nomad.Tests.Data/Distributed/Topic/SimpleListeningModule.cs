using System;
using System.IO;
using System.Reflection;
using Nomad.Communication.EventAggregation;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.Topic
{
	/// <summary>
	///		Simple module for listening to data
	/// </summary>
	public class SimpleListeningModule : Nomad.Modules.IModuleBootstraper
	{
		private readonly IEventAggregator _eventAggregator;
		private int _counter;
		private FileInfo _fileInfo;

		public SimpleListeningModule(IEventAggregator eventAggregator)
		{
			_counter = 0;
			_eventAggregator = eventAggregator;
		}

		public void OnLoad()
		{
			_eventAggregator.Subscribe<DistributableMessage>(CallBack);
			_fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location+"_CounterFile");
			_fileInfo.Delete();
		}


		private void CallBack(DistributableMessage obj)
		{
			++_counter;
			if (_counter >=5)
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
