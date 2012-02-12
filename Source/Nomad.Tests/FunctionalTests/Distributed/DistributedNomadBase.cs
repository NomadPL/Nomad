using System;
using System.Diagnostics;
using System.IO;
using Nomad.Core;
using Nomad.KeysGenerator;
using Nomad.Tests.FunctionalTests.Fixtures;
using NUnit.Framework;
using TestsShared.Utils;

namespace Nomad.Tests.FunctionalTests.Distributed
{
	public class DistributedNomadBase
	{
		protected readonly ModuleCompiler Compiler = new ModuleCompiler();
		protected NomadKernel ListenerKernel;
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
			KeysGeneratorProgram.Main(new[] { KeyFile });
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

			if (PublisherKernel != null)
			{
				PublisherKernel.Dispose();
			}
		}

		protected String KeyFile;

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
			return new StackTrace().GetFrame(1).GetMethod().Name;
		}
	}
}