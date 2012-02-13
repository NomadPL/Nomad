using System;
using System.IO;
using System.Reflection;
using Nomad.Communication.EventAggregation;
using Nomad.Tests.Data.Distributed.Commons;

namespace Nomad.Tests.Data.Distributed.SingleDelivery
{
	/// <summary>
	///		Simple module for listening to data
	/// </summary>
	public class SDListeningModule : Modules.IModuleBootstraper
	{
		private readonly IEventAggregator _eventAggregator;

		public SDListeningModule(IEventAggregator eventAggregator)
		{
			_eventAggregator = eventAggregator;
		}

		public void OnLoad()
		{
			_eventAggregator.Subscribe<DistributableMessage>(CallBack);
		}

		private static void CallBack(DistributableMessage obj)
		{
			string myName = Assembly.GetExecutingAssembly().GetName().Name;
			DistributedMessageRegistry.Add(myName);
			Console.WriteLine("Added message '{0}' to the registry",myName);
		}

		public void OnUnLoad()
		{
			// do nothing
		}
	}
}
