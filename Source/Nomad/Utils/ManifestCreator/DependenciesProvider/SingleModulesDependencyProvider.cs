using System;
using System.Collections.Generic;
using System.Reflection;
using Nomad.Modules.Manifest;

namespace Nomad.Utils.ManifestCreator.DependenciesProvider
{
	/// <summary>
	///		Provides the only the dependencies which where provided during initalization
	/// of this object.
	/// </summary>
	public class SingleModulesDependencyProvider : IModulesDependenciesProvider
	{
		private readonly string[] _pathToAssembly;
		private List<ModuleDependency> _dependencies;

		public SingleModulesDependencyProvider(params string[] pathToAssembly)
		{
			_pathToAssembly = pathToAssembly;
		}

		#region IModulesDependenciesProvider Members

		public IEnumerable<ModuleDependency> GetDependencyModules(string directory, string assemblyPath)
		{
			_dependencies = new List<ModuleDependency>();

			foreach (string file in _pathToAssembly)
			{
				AssemblyName asmName = null;
				try
				{
					asmName = AssemblyName.GetAssemblyName(file);
				}
				catch (BadImageFormatException)
				{
					// nothing happens, return empty list
					continue;
				}
				catch (Exception e)
				{
					throw new InvalidOperationException("Access to file is somewhat corrupted", e);
				}

				var dependency = new ModuleDependency
				                 	{
				                 		ModuleName = asmName.Name,
				                 		MinimalVersion = new Version(asmName.Version),
				                 		ProcessorArchitecture = asmName.ProcessorArchitecture,
				                 		HasLoadingOrderPriority = false,
				                 	};

				_dependencies.Add(dependency);
			}


			return _dependencies;
		}

		#endregion
	}
}