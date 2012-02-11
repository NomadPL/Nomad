using System;
using System.IO;
using Nomad.Core;
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
		private string _sourceDir;

		protected string SourceFolder
		{
			get { return @"..\Source\" + _sourceDir; }
		}

		protected void SetSourceFolder(Type type)
		{
			_sourceDir = FileHelper.GetNamespaceSourceFolder(type);
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

		protected String GetSourceCodePath(String codeLocation)
		{
			string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SourceFolder);
			return Path.Combine(dirPath, codeLocation);
		}
	}
}