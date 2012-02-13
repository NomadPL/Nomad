using System.IO;
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
			_eventAggregator.Subscribe<PathMessage>(SetPath);
			_eventAggregator.Subscribe<DistributableMessage>(CallBack);
		}

		private void SetPath(PathMessage obj)
		{
			_fileInfo = new FileInfo(Path.Combine(obj.Payload, "CounterFile"));
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
