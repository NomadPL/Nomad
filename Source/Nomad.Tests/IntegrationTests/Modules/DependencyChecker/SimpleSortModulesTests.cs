using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using Nomad.Modules;
using Nomad.Modules.Manifest;
using NUnit.Framework;
using TestsShared;
using Version = Nomad.Utils.Version;

namespace Nomad.Tests.IntegrationTests.Modules.DependencyChecker
{
    /// <summary>
    ///     Tests are performed with purpose of checking only dependency resolving using 
    ///     <see cref="ModuleManifest.ModuleDependencies"/> of <see cref="ModuleManifest"/> with:
    ///     <see cref="ModuleDependency.ModuleName"/> and <see cref="ModuleDependency.HasLoadingOrderPriority"/> properties.
    /// </summary>
    [IntegrationTests]
    public class SimpleSortModulesTests : DependencyCheckerBase
    {

        [SetUp]
        public void set_up()
        {
            DependencyChecker = new Nomad.Modules.DependencyChecker();
        }


        [Test]
        public void sorting_empty_list()
        {
            Modules = new List<ModuleInfo>();
            ExpectedModules = new List<ModuleInfo>();

            IEnumerable<ModuleInfo> result = DependencyChecker.SortModules(Modules);

            Assert.AreEqual(ExpectedModules, result);
        }

        [Test]
        public void sorting_one_elment_list()
        {
            var a = SetUpModuleInfo("A");
            Modules = new List<ModuleInfo>() {a};
            ExpectedModules = new List<ModuleInfo>() {a};

            Assert.AreEqual(ExpectedModules,DependencyChecker.SortModules(Modules),"One element list should be sorted right away");
        }

        [Test]
        public void sorting_two_independnt_element_list()
        {
            var a = SetUpModuleInfo("A");
            var b = SetUpModuleInfo("B");
            Modules = new List<ModuleInfo>() { a,b };
            ExpectedModules = new List<ModuleInfo>() { a,b };

            Assert.AreEqual(ExpectedModules, DependencyChecker.SortModules(Modules), "Two element list with no dependencies should be sorted right away");
        }

        [Test]
        public void sorting_hard_linked_dag_list()
        {
            /* The graph:
             * A -> B -> C - > X
             *   \            /
             *    > D -> E  /
             */

            var a = SetUpModuleInfo("A", "B", "D");
            var b = SetUpModuleInfo("B", "C");
            var d = SetUpModuleInfo("D", "E");
            var c = SetUpModuleInfo("C", "X");
            var e = SetUpModuleInfo("E", "X");
            var x = SetUpModuleInfo("X");

            Modules = new List<ModuleInfo>
                           {
                               
                               b,
                               a,
                               d,
                               c,
                               x,
                               e,
                           };

            ExpectedModules = new List<ModuleInfo>
                                   {
                                       x,
                                       c,
                                       b,
                                       e,
                                       d,
                                       a
                                   };

            // perform test
            Assert.AreEqual(ExpectedModules, DependencyChecker.SortModules(Modules));
        }


        [Test]
        public void sorting_hard_linked_non_dag_list_results_in_exception()
        {
            /*
             * The non-dag graph:
             * A -> B -> C -> A
             * |
             * > X -> Y
             */
            var a = SetUpModuleInfo("A", "B", "X");
            var b = SetUpModuleInfo("B", "C");
            var c = SetUpModuleInfo("C", "A");
            var e = SetUpModuleInfo("X", "Y");
            var x = SetUpModuleInfo("Y");

            Modules = new List<ModuleInfo>()
                           {
                               a,
                               b,
                               c,
                               e,
                               x
                           };
            ExpectedModules = null;

            // perform test 
            Assert.Throws<ArgumentException>(() => DependencyChecker.SortModules(Modules));
        }

        [Test]
        public void sorting_contains_cycle_exception()
        {
            /*
             * A -> B -> C -> B
             */
            var a = SetUpModuleInfo("A", "B");
            var b = SetUpModuleInfo("B", "C");
            var c = SetUpModuleInfo("C", "B");

            Modules = new List<ModuleInfo> { a, b, c };

            ExpectedModules = null;

            // perform test
            Assert.Throws<ArgumentException>(() => DependencyChecker.SortModules(Modules));
        }
    }
}