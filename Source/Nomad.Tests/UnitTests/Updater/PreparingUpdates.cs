using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Nomad.Messages;
using Nomad.Messages.Updating;
using Nomad.Modules;
using Nomad.Modules.Manifest;
using Nomad.Updater;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.UnitTests.Updater
{
	[UnitTests]
	public class PreparingUpdates : UpdaterTestFixture
	{
		private ModuleManifest _moduleManifest;
		private string _moduleName;


		[SetUp]
		public void set_up()
		{
			_moduleName = "test_module.dll";
			var moduleVersion = new Nomad.Utils.Version("0.1.1.0");

			_moduleManifest = new ModuleManifest {ModuleName = _moduleName, ModuleVersion = moduleVersion};

			// sick creating of dependency checker
			IEnumerable<ModuleInfo> outter = new List<ModuleInfo>();
			DependencyChecker.Setup(
				x =>
				x.CheckModules(It.IsAny<IEnumerable<ModuleInfo>>(),
							   It.IsAny<IEnumerable<ModuleInfo>>(),
							   out outter))
				.Returns(true);
		}

		[Test]
		public void packages_gotten_from_repository_have_nulls()
		{
			EventAggregator.Setup(x => x.Publish(It.IsAny<NomadUpdatesReadyMessage>()))
				.Callback<NomadUpdatesReadyMessage>(msg => Assert.IsTrue(msg.Error))
				.Returns(null)
				.Verifiable("The message should be published");

			var modulePackages = new List<ModuleManifest>
									{
										new ModuleManifest() {ModuleName = "Test"},
									};

			// provide wrong packages in repository
			ModulesRepository.Setup(x => x.GetModule(It.IsAny<string>())).Returns(
				new ModulePackage());

			NomadUpdater.PrepareUpdate(modulePackages);

			EventAggregator.Verify();
			Assert.AreEqual(UpdaterStatus.Invalid, NomadUpdater.Status);
		}

		[Test]
		public void preparing_updates_invoked_with_null_raises_message()
		{
			EventAggregator.Setup(x => x.Publish(It.IsAny<NomadUpdatesReadyMessage>()))
				.Callback<NomadUpdatesReadyMessage>(msg => Assert.IsTrue(msg.Error))
				.Returns(null)
				.Verifiable("The message should be published");

			NomadUpdater.PrepareUpdate(null);

			EventAggregator.Verify();
			Assert.AreEqual(UpdaterStatus.Invalid, NomadUpdater.Status);
		}

		[Test]
		public void module_repository_throws_exception_while_preparing_updates_changed_to_message()
		{
			ModulesRepository.Setup(x => x.GetModule(It.IsAny<string>()))
				.Throws(new Exception("Module Cannot be selected"));

			EventAggregator.Setup(x => x.Publish(It.IsAny<NomadUpdatesReadyMessage>()))
				.Callback<NomadUpdatesReadyMessage>(msg => Assert.IsTrue(msg.Error))
				.Returns(null)
				.Verifiable("The message should be published");

			NomadUpdater.PrepareUpdate(new List<ModuleManifest>()
										{
											_moduleManifest
										});

			EventAggregator.Verify();
			Assert.AreEqual(UpdaterStatus.Invalid, NomadUpdater.Status);
		}

		[Test]
		public void preparing_updates_calls_proper_subsystems_publishes_msg_at_the_end()
		{
			// return the same - might stop working if some of the code is gonna be put to verify download };
			var modulePacakge = new ModulePackage {ModuleManifest = _moduleManifest};

			ModulesRepository.Setup(x => x.GetModule(It.Is<string>(y => y == _moduleName)))
				.Returns(modulePacakge)
				.Verifiable("This package should be downloaded");

			EventAggregator.Setup(x => x.Publish(It.IsAny<NomadUpdatesReadyMessage>()))
				.Callback<NomadUpdatesReadyMessage>(msg =>
														{
															Assert.IsFalse(msg.Error);
															Assert.AreEqual(1, msg.ModuleManifests.Count);
															Assert.AreSame(modulePacakge.ModuleManifest,
																		   msg.ModuleManifests[0]);
														})
				.Returns(null)
				.Verifiable("This message should be published upon exit");

			NomadUpdater.PrepareUpdate(new List<ModuleManifest>()
										{
											_moduleManifest
										});

			EventAggregator.Verify();
			ModulesRepository.Verify();
			Assert.AreEqual(UpdaterStatus.Prepared, NomadUpdater.Status);
		}
	}
}