using System.IO;
using System.Reflection;
using Nomad.Communication.EventAggregation;
using Nomad.Modules;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.TimeBuffered
{
    /// <summary>
    ///		Simple module for listening to data
    /// </summary>
    public class SimpleListeningModule : IModuleBootstraper
    {
        private readonly IEventAggregator _eventAggregator;
        private int _counter;
        private FileInfo _fileInfo;

        public SimpleListeningModule(IEventAggregator eventAggregator)
        {
            _counter = 0;
            _eventAggregator = eventAggregator;
        }

        #region IModuleBootstraper Members

        public void OnLoad()
        {
            _fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location + "_CounterFile");
            _fileInfo.Delete();
            _eventAggregator.Subscribe<DistributableMessage>(CallBack);
        }


        public void OnUnLoad()
        {
        }

        #endregion

        private void CallBack(DistributableMessage obj)
        {
            ++_counter;
            if (_counter >= 5)
            {
                using (StreamWriter text = _fileInfo.CreateText())
                {
                    text.WriteLine(_counter);
                    text.Close();
                }
            }
        }
    }
}