using System;
using System.Collections.Generic;
using System.Linq;
using Nomad.Modules.Manifest;

namespace Nomad.Utils.ManifestCreator.DependenciesProvider
{
	/// <summary>
	///		Enables the composite pattern on <see cref="IModulesDependenciesProvider"/>
	/// various implementations.
	/// </summary>
	public class CompositeModulesDependencyProvider : IModulesDependenciesProvider
	{
		private readonly IModulesDependenciesProvider[] _providers;

		public CompositeModulesDependencyProvider(params IModulesDependenciesProvider[] providers)
		{
			_providers = providers;
		}

		public IEnumerable<ModuleDependency> GetDependencyModules(string directory, string assemblyPath)
		{
			var finalList = new List<ModuleDependency>();
			foreach (var provider in _providers)
			{
				var list = provider.GetDependencyModules(directory, assemblyPath);
				finalList = finalList.Concat(list).ToList();
			}

			return finalList;
		}
	}
}