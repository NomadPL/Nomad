using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nomad.Core;
using Nomad.KeysGenerator;
using Nomad.Modules;
using Nomad.Modules.Discovery;
using Nomad.Modules.Filters;
using Nomad.Tests.FunctionalTests.Modules;
using Nomad.Utils.ManifestCreator;
using Nomad.Utils.ManifestCreator.DependenciesProvider;
using NUnit.Framework;

namespace Nomad.Tests.FunctionalTests.Fixtures
{
    public class ModuleLoadingWithCompilerTestFixture : MarshalByRefObject
    {
        private const string KeyFile = @"alaMaKota.xml";
        private ModuleCompiler _moduleCompiler;

        protected NomadKernel Kernel;
        protected AppDomain Domain;


    	public ModuleCompiler ModuleCompiler
    	{
    		get { return _moduleCompiler; }
    	}

    	[TestFixtureSetUp]
        public virtual void SetUpFixture()
        {
            if (File.Exists(KeyFile))
            {
                File.Delete(KeyFile);
            }
            KeysGeneratorProgram.Main(new[] {KeyFile});

            _moduleCompiler = new ModuleCompiler();
        }

        [TestFixtureTearDown]
        public virtual void CleanUpFixture()
        {
            if (File.Exists(KeyFile))
            {
                File.Delete(KeyFile);
            }
        }


        [SetUp]
        public virtual void SetUp()
        {
            // prepare configuration
            NomadConfiguration configuration = NomadConfiguration.Default;
            configuration.ModuleFilter = new CompositeModuleFilter();
            configuration.DependencyChecker = new DependencyChecker();

            // initialize kernel
            Kernel = new NomadKernel(configuration);

            // domain
            Domain = Kernel.ModuleAppDomain;
        }


        protected void LoadModulesFromDiscovery(IModuleDiscovery discovery)
        {
            Kernel.LoadModules(discovery);
        }


        protected void AssertModulesLoadedAreEqualTo(params string[] expectedModuleNames)
        {
            // cross domain communication is really painful
            var carrier =
                (MessageCarrier)
                Domain.CreateInstanceAndUnwrap(typeof (MessageCarrier).Assembly.FullName,
                                               typeof (MessageCarrier).FullName);

            string[] modules = carrier.List.ToArray();
            Assert.That(modules, Is.EqualTo(expectedModuleNames));
        }

        #region Nested type: MessageCarrier

        private class MessageCarrier : MarshalByRefObject
        {
            private readonly IList<string> _list;


            public MessageCarrier()
            {
                _list = LoadedModulesRegistry.GetRegisteredModules()
                    .Select(type => type.Name).ToList();
            }


            public IEnumerable<string> List
            {
                get { return _list; }
            }
        }

        #endregion
    }
}