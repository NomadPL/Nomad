using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nomad.KeysGenerator;
using Nomad.Modules.Manifest;
using Nomad.Signing;
using Nomad.Utils.ManifestCreator;
using Nomad.Utils.ManifestCreator.DependenciesProvider;

namespace Nomad.Tests.FunctionalTests.Fixtures
{
	/// <summary>
	///     Generates the module assembly from source files.
	/// </summary>
	/// <remarks>
	///     Puts the additional Nomad dependency nad Nomad.Tests dependency alsow.
	/// TODO: make the <see cref="ModuleCompiler"/> more configurable class.
	/// </remarks>
	public class ModuleCompiler
	{
		private static readonly string NomadAssembly = AppDomain.CurrentDomain.BaseDirectory +
		                                               @"\Nomad.dll";

		private static readonly string NomadTestAssembly = AppDomain.CurrentDomain.BaseDirectory +
		                                                   @"\Nomad.Tests.dll";

		private readonly CodeDomProvider _provider;
		private string _outputDirectory;
		private CompilerParameters _parameters;
		private CompilerResults _results;


		public ModuleCompiler()
		{
			_provider = CodeDomProvider.CreateProvider("CSharp");
		}

		/// <summary>
		///     Represents the output path with name to the created assembly.
		/// </summary>
		public string OutputName { get; set; }

		/// <summary>
		///     Represents the output directory where the build artifacts will be put.
		/// </summary>
		public string OutputDirectory
		{
			get
			{
				if (string.IsNullOrEmpty(_outputDirectory))
					_outputDirectory = ".";
				return _outputDirectory;
			}

			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (!Directory.Exists(value))
						Directory.CreateDirectory(value);
				}
				_outputDirectory = value;
			}
		}

		/// <summary>
		///     Gets the path to SimplestModulePossible1.
		/// </summary>
		public static string DefaultSimpleModuleSource
		{
			get { return @"..\Source\Nomad.Tests\FunctionalTests\Data\SimplestModulePossible1.cs"; }
		}

		/// <summary>
		///     Gets the alternative path. Thus SimplestModulePossible2.
		/// </summary>
		public static string DefaultSimpleModuleSourceAlternative
		{
			get { return @"..\Source\Nomad.Tests\FunctionalTests\Data\SimplestModulePossible2.cs"; }
		}

		/// <summary>
		///     Gets the alternative, alternative path. Thus SimplestModulePossible3
		/// </summary>
		public static string DefaultSimpleModuleSourceLastAlternative
		{
			get { return @"..\Source\Nomad.Tests\FunctionalTests\Data\SimplestModulePossible3.cs"; }
		}

		/// <summary>
		///     Generates the module from <paramref name="sourceFilePath"/> with the additional assemblies presented in <paramref name="dependeciesAssembliesPath"/>.
		/// </summary>
		/// <remarks>
		///		When no <see cref="OutputName"/> is provided then the name of the source file with the <c>.dll</c> ending is used. When name <c>is provided</c>
		/// the <see cref="OutputDirectory"/> property <c>xwill not be used</c>.
		/// </remarks>
		/// <param name="sourceFilePath">source file </param>
		/// <param name="dependeciesAssembliesPath">Array of the dependecies (assemblies) to be the following assembly dependent on.</param>
		/// <returns>The path to the compiled assembly.</returns>
		public string GenerateModuleFromCode(string sourceFilePath,
		                                     params string[] dependeciesAssembliesPath)
		{
			return GenerateModuleFromCode(new[] {sourceFilePath}, dependeciesAssembliesPath);
		}

		public string GenerateModuleFromCode(string[] sourceFilesPath, params string[] dependeciesAssembliesPath)
		{
			IEnumerable<string> asmReferences =
				dependeciesAssembliesPath.ToList().Concat(new List<string>
				                                          	{
				                                          		NomadAssembly,
				                                          		NomadTestAssembly
				                                          	});
			if (string.IsNullOrEmpty(OutputName))
			{
				// the name is the first element
				OutputName = Path.Combine(OutputDirectory,
				                          Path.GetFileNameWithoutExtension(sourceFilesPath[0]) + ".dll");
			}


			_parameters = new CompilerParameters(asmReferences.ToArray())
			              	{
			              		GenerateExecutable = false,
			              		TreatWarningsAsErrors = false,
			              		OutputAssembly = OutputName,
			              	};


			string[] srcPaths = sourceFilesPath.Select(x => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, x)).ToArray();
			_results = _provider.CompileAssemblyFromFile(_parameters, srcPaths);

			if (_results.Errors.Count > 0)
			{
				foreach (CompilerError error in _results.Errors)
				{
					Console.WriteLine(error);
				}
				throw new Exception("Compilation exception during compiling modules");
			}

			OutputName = null;

			return _parameters.OutputAssembly;
		}


		/// <summary>
		///     Wrapps the generating manifest with values that should be provided. Provides access to <paramref name="configuration"/>.
		/// </summary>
		/// <param name="modulePath"></param>
		/// <param name="keyLocation"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public string GenerateManifestForModule(string modulePath, string keyLocation,
		                                        ManifestBuilderConfiguration configuration)
		{
			string directory = Path.GetFullPath(Path.GetDirectoryName(modulePath));
			var builder = new ManifestBuilder("TEST_ISSUER_COMPILER",
			                                  keyLocation,
			                                  Path.GetFileName(modulePath), directory, KeyStorage.Nomad, string.Empty,
			                                  configuration);
			builder.CreateAndPublish();

			return modulePath + ModuleManifest.ManifestFileNameSuffix;
		}

		/// <summary>
		///		Generate manifest for module.
		/// </summary>
		public string GenerateManifestForModule(string modulePath, string keyLocation)
		{
			return GenerateManifestForModule(modulePath, keyLocation, ManifestBuilderConfiguration.Default);
		}

		/// <summary>
		///		Wrapps up the generating manifest. Uses <see cref="WholeDirectoryModulesDependenciesProvider"/> with
		/// the <see cref="ManifestBuilderConfiguration"/> file.
		/// </summary>
		public string SetUpModuleWithManifest(string outputDirectory, string srcPath,
		                                      params string[] references)
		{
			// NOTE: we are using whole directory module discovery instead of file one
			ManifestBuilderConfiguration configuration = ManifestBuilderConfiguration.Default;
			configuration.ModulesDependenciesProvider = new WholeDirectoryModulesDependenciesProvider();

			return SetUpModuleWithManifest(outputDirectory, srcPath, configuration, references);
		}

		/// <summary>
		///		Generates the manifest for the file with coping all the dependencies and then removing them
		/// </summary>
		/// <remarks>
		///		This method will be problematic with duplcate names and so on...
		/// </remarks>
		/// <param name="references">Path to all references</param>
		///<returns>
		///		The path to the generated manifest
		/// </returns>
		public string SetUpModuleWithManifest(string outputDirectory, string srcPath,
		                                      ManifestBuilderConfiguration configuration,
		                                      params string[] references)
		{
			OutputDirectory = outputDirectory;

			string modulePath = GenerateModuleFromCode(srcPath, references);

			// copy the references into folder with 
			foreach (string reference in references)
			{
				File.Copy(reference, Path.Combine(outputDirectory, Path.GetFileName(reference)));
			}

			string KeyFile = @"alaMaKota.xml";
			if (File.Exists(KeyFile))
			{
				File.Delete(KeyFile);
			}
			KeysGeneratorProgram.Main(new[] {KeyFile});

			// manifest generating is for folder
			string file = GenerateManifestForModule(modulePath, KeyFile, configuration);

			if (File.Exists(KeyFile))
			{
				File.Delete(KeyFile);
			}

			// remove those references
			foreach (string reference in references)
			{
				File.Delete(Path.Combine(outputDirectory, Path.GetFileName(reference)));
			}

			return file;
		}
	}
}