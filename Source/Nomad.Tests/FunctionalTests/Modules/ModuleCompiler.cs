﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nomad.Modules.Manifest;
using Nomad.Utils;

namespace Nomad.Tests.FunctionalTests.Modules
{
    /// <summary>
    ///     Generates the module assembly from source files.
    /// </summary>
    /// <remarks>
    ///     Puts the additional Nomad dependency.
    /// </remarks>
    public class ModuleCompiler
    {
        public static readonly string NomadAssembly = AppDomain.CurrentDomain.BaseDirectory +
                                                      @"\Nomad.dll";

        public static readonly string NomadTestAssembly = AppDomain.CurrentDomain.BaseDirectory +
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
        ///     Generates the module from <paramref name="sourceFilePath"/> with the additional assemblies presented in <paramref name="dependeciesAssembliesPath"/>.
        /// </summary>
        /// <param name="sourceFilePath">ource file </param>
        /// <param name="dependeciesAssembliesPath">Array of the dependecies (assemblies) to be the following assembly dependent on.</param>
        /// <returns>The path to the compiled assembly.</returns>
        public string GenerateModuleFromCode(string sourceFilePath,
                                             params string[] dependeciesAssembliesPath)
        {
            IEnumerable<string> asmReferences =
                dependeciesAssembliesPath.ToList().Concat(new List<string>
                                                              {
                                                                  NomadAssembly,
                                                                  NomadTestAssembly
                                                              });

            _parameters = new CompilerParameters(asmReferences.ToArray())
                              {
                                  GenerateExecutable = false,
                                  TreatWarningsAsErrors = false,
                                  OutputAssembly = Path.Combine(OutputDirectory,
                                                                Path.GetFileNameWithoutExtension(
                                                                    sourceFilePath) + ".dll")
                              };

            string srcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceFilePath);
            _results = _provider.CompileAssemblyFromFile(_parameters, srcPath);

            if (_results.Errors.Count > 0)
            {
                foreach (CompilerError  error in _results.Errors)
                {
                    Console.WriteLine(error);
                }
                throw new Exception("Compilation exception during compiling modules");
            }

            return _parameters.OutputAssembly;
        }


        public string GenerateManifestForModule(string modulePath,string keyLocation)
        {
            string directory = Path.GetFullPath(Path.GetDirectoryName(modulePath));
            var builder = new ManifestBuilder("ALAMAKOTA",
                                              keyLocation,
                                              Path.GetFileName(modulePath), directory);
            builder.Create();

            return modulePath + ModuleManifest.ManifestFileNameSuffix;
        }
    }
}