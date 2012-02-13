using System;
using System.Diagnostics;
using System.IO;
using Nomad.Core;
using Nomad.KeysGenerator;
using Nomad.Tests.Data.Distributed.Commons;
using Nomad.Tests.FunctionalTests.Fixtures;
using NUnit.Framework;
using Nomad.Utils.ManifestCreator;
using Nomad.Utils.ManifestCreator.DependenciesProvider;
using TestsShared.Utils;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	/// <summary>
	///		Helper base class for all types of tests. 
	/// 
	/// <para>
	/// Includes: 
	/// <list type="bullet" | "number" | "table">
	/// 
	/// 
	/// <item>
	///		basic set up of <see cref="NomadKernel"/> instances
	/// </item>
	/// <item>
	///		proper path transforming mechanisms
	/// </item>	
	/// <item>
	///		Carrier for sending the information about messages
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	public class DistributedNomadBase
	{
		private const string RUNTIME_LOCATION = @"Modules\Distributed\";
		protected const int PUBLISH_TIMEOUT = 1500;

		protected readonly ModuleCompiler Compiler = new ModuleCompiler();
		protected NomadKernel ListenerKernel;
		protected NomadKernel ListenerKernelSecond;
		protected NomadKernel PublisherKernel;
		private string _sourceDir = string.Empty;

		protected string SourceFolder
		{
			get { return @"..\Source\" + _sourceDir; }
		}

		protected void SetSourceFolder(Type type)
		{
			_sourceDir = FileHelper.GetNamespaceSourceFolder(type);
		}

		[TestFixtureSetUp]
		public void fixture_set_up()
		{
			KeyFile = @"alaMaKota.xml";
			if (File.Exists(KeyFile))
			{
				File.Delete(KeyFile);
			}
			KeysGeneratorProgram.Main(new[] {KeyFile});
		}

		[TestFixtureTearDown]
		public void fixture_tear_down()
		{
			if (File.Exists(KeyFile))
			{
				File.Delete(KeyFile);
			}
		}

		[TearDown]
		public void tear_down()
		{
			if (ListenerKernel != null)
			{
				ListenerKernel.Dispose();
			}

			if (ListenerKernelSecond != null)
			{
				ListenerKernelSecond.Dispose();
			}

			if (PublisherKernel != null)
			{
				PublisherKernel.Dispose();
			}
		}

		protected String KeyFile;
		protected string _sharedDll;
		protected string _runtimePath;

		protected String GetSourceCodePath(String codeLocation)
		{
			string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SourceFolder);
			return Path.Combine(dirPath, codeLocation);
		}

		protected String GetSourceCodePath(Type type)
		{
			string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SourceFolder);
			return Path.Combine(dirPath, FileHelper.GetClassSourceFile(type));
		}

		protected static String GetCurrentMethodName()
		{
			return new StackTrace().GetFrame(2).GetMethod().Name;
		}

		protected String GetCurrentClassName()
		{
			return this.GetType().Name;
		}

		protected void PrepareSharedLibrary()
		{

			_runtimePath = RUNTIME_LOCATION + GetCurrentClassName() + "\\" + GetCurrentMethodName();

			// remove the directory if it is there already
			if (Directory.Exists(_runtimePath))
			{
				Directory.Delete(_runtimePath, true);	
			}

			string sharedModuleSrc = GetSourceCodePath(typeof (DistributableMessage));
			string sharedRegistry = GetSourceCodePath(typeof (DistributedMessageRegistry));
			_sharedDll = Compiler.GenerateModuleFromCode(new[] {sharedModuleSrc,sharedRegistry});
			Compiler.OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _runtimePath);
		}

		protected string GenerateListener(string runtimePath, string sharedDll, string listeningModuleSrc, int counter)
		{
			Compiler.OutputName = Path.Combine(runtimePath, "listener" + counter + ".dll");
			string listenerDll = Compiler.GenerateModuleFromCode(listeningModuleSrc, sharedDll);
			ManifestBuilderConfiguration manifestConfiguration = ManifestBuilderConfiguration.Default;
			manifestConfiguration.ModulesDependenciesProvider = new SingleModulesDependencyProvider();
			Compiler.GenerateManifestForModule(listenerDll, KeyFile, manifestConfiguration);

			return listenerDll;
		}

		protected static DistributedMessageCarrier CreateCarrier(NomadKernel kernel)
		{
			var asmName = typeof (DistributedMessageCarrier).Assembly.FullName;
			var typeName = typeof (DistributedMessageCarrier).FullName;
			var carrier = (DistributedMessageCarrier) kernel.ModuleAppDomain.CreateInstanceAndUnwrap(asmName, typeName);


			return carrier;
		}
	}
}