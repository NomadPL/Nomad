using System;
using System.IO;
using Nomad.KeysGenerator;
using Nomad.Tests.FunctionalTests.Fixtures;
using NUnit.Framework;
using TestsShared;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	/// <summary>
	///		Tests the <see cref="Nomad.Distributed"/> mechanisms at the functional 
	/// level of testing
	/// </summary>
	[FunctionalTests]
	public class TopicDistributedNomad
	{
		private ModuleCompiler _compiler = new ModuleCompiler();
		private string _runtimePath;
		private const string KeyFile = @"KEY_FILE.xml";
		private const string RUNTIME_DIR = @"FunctionalTests\Distributed\";
		private const string SOURDE_DIR = @"..\Source\Nomad.Tests\FunctionalTests\Distributed\Data";

		[TestFixtureSetUp]
		public void fixture_set_up()
		{
			if (File.Exists(KeyFile))
			{
				File.Delete(KeyFile);
			}
			KeysGeneratorProgram.Main(new[] {KeyFile});
		}

		[SetUp]
		public void set_up()
		{
			// place where module for testing 
			_runtimePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RUNTIME_DIR);
		}

		private static String GetSourceCodePath(String codeLocation)
		{
			var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SOURDE_DIR);
			return Path.Combine(dirPath, codeLocation);
		}

		private String Compile(string outputName, string pathToCode, params string[] dependecies)
		{
			_compiler.OutputDirectory = _runtimePath;
			_compiler.OutputName = outputName;
			var modulePath = _compiler.GenerateModuleFromCode(pathToCode, dependecies);
			_compiler.GenerateManifestForModule(modulePath, KeyFile);

			return modulePath;
		}


		[Ignore("Quite hard to implement properly")]
		public void module_publishes_module_listens()
		{
			// TODO: this code is not refactor aware
			string sharedInterface = GetSourceCodePath(@"DistributableMessage.cs");
			string module1SourceCode = GetSourceCodePath(@"SimplePublishingModule.cs");
			string module2SourceCode = GetSourceCodePath(@"SimpleListeningModule.cs");

			// prepare module generating messages
			var sharedModulePath = Compile("shared.dll", sharedInterface);
			Compile("m1.dll", module1SourceCode, sharedModulePath);
			Compile("m2.dll", module2SourceCode, sharedModulePath);

			// create kernel for those modules 
			
		}
	}
}