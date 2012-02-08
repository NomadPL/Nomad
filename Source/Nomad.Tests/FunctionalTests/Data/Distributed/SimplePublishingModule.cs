using System;
using System.IO;
using Nomad.Communication.EventAggregation;
using Nomad.Messages.Loading;
using Nomad.Tests.FunctionalTests.Kernel.Messages;

public class SimplePublishingModule : Nomad.Modules.IModuleBootstraper
{
	private IEventAggregator _aggregator;

	public SimplePublishingModule(IEventAggregator eventAggregator)
	{
		_aggregator = eventAggregator;
	}

	public void OnLoad()
	{
		_aggregator.Subscribe<NomadCouldNotLoadModuleMessage>(
			delegate { EventHandledRegistry.RegisterEventType(typeof(EventAwareModule)); });
	}

	public void OnUnLoad()
	{
		// do nothing
	}
}
