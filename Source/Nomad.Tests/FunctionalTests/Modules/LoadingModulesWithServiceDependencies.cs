using System;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nomad.KeysGenerator;
using Nomad.Modules;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Modules
{
    [FunctionalTests]
    public class LoadingModulesWithServiceDependencies : ModuleLoadingWithCompilerTestFixture
    {
        [Test]
        public void module_loader_discovers_and_loads_all_simple_modules()
        {
            LoadModulesFromDirectory(@"Modules\WithDependencies\");
            AssertModulesLoadedAreEqualTo("ModuleWithConstructorDependency",
                                          "ModuleWithPropertyDependency");
        }
    }
}