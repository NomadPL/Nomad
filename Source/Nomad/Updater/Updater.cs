﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nomad.Communication.EventAggregation;
using Nomad.Core;
using Nomad.Messages.Updating;
using Nomad.Modules;
using Nomad.Modules.Discovery;
using Nomad.Modules.Manifest;
using Nomad.Updater.ModulePackagers;
using Nomad.Updater.ModuleRepositories;

namespace Nomad.Updater
{
    /// <summary>
    ///     Manages the process of update.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Updater uses various engines to perform its work, the most significant are:
    /// <see cref="IModulesRepository"/> - repository that describes the placement of the updates.
    /// <see cref="IDependencyChecker"/> - engine for checking validity of the provided updates.
    /// <see cref="IModulePackager"/> - engine for unpacking data downloaded from repository.
    /// </para>
    /// <para>
    ///     Nevertheless of messages passed via <see cref="IEventAggregator"/> updater signals its state via <see cref="Status"/> property.
    /// </para>
    /// <para>
    ///     Updater provides two message types via EventAggregator:
    ///     <see cref="NomadAvailableUpdatesMessage"/> for signaling available updates.
    /// and <see cref="NomadUpdatesReadyMessage"/> for signaling that updates are prepared for installation - downloaded.
    /// </para>
    /// <para>
    ///     TODO: write about thread safety and implement lock on status object.
    /// </para>
    /// </remarks>
    public class Updater : MarshalByRefObject, IUpdater
    {
        private readonly IDependencyChecker _dependencyChecker;
        private readonly IEventAggregator _eventAggregator;
        private readonly IModuleManifestFactory _moduleManifestFactory;
        private readonly IModulePackager _modulePackager;
        private readonly IModulesOperations _modulesOperations;
        private readonly IModulesRepository _modulesRepository;
        private readonly string _targetDirectory;
        private IModuleDiscovery _defaultAfterUpdateModules;

        private IEnumerable<ModulePackage> _modulesPackages;


        private AutoResetEvent _updateFinished;


        /// <summary>
        ///     Initializes updater instance with proper engines.
        /// </summary>
        /// <param name="targetDirectory">directory to install modules to</param>
        /// <param name="modulesRepository">backend used to retrieve modules information. ie. WebServiceModulesRepository, WebModulesRepository, and so on</param>
        /// <param name="modulesOperations">backend used to unload / load modules</param>
        /// <param name="moduleManifestFactory">factory which creates <see cref="ModuleManifest"/> based on <see cref="ModuleInfo"/></param>
        /// <param name="eventAggregator">event aggregator for providing events</param>
        /// <param name="modulePackager">packager used for unpacking packages</param>
        /// <param name="dependencyChecker">dependency checker engine used for validating the outcome</param>
        public Updater(string targetDirectory, IModulesRepository modulesRepository,
                       IModulesOperations modulesOperations,
                       IModuleManifestFactory moduleManifestFactory,
                       IEventAggregator eventAggregator, IModulePackager modulePackager,
                       IDependencyChecker dependencyChecker)
        {
            Status = UpdaterStatus.Idle;

            _targetDirectory = targetDirectory;
            _dependencyChecker = dependencyChecker;
            _modulePackager = modulePackager;
            _modulesRepository = modulesRepository;
            _modulesOperations = modulesOperations;
            _moduleManifestFactory = moduleManifestFactory;
            _eventAggregator = eventAggregator;

            _modulesPackages = new List<ModulePackage>();
        }

        #region IUpdater Members

        public AutoResetEvent UpdateFinished
        {
            get { return _updateFinished; }
            private set { _updateFinished = value; }
        }

        /// <summary>
        ///     Describes the result of former update. 
        /// </summary>
        public UpdaterStatus Status { get; private set; }

        /// <summary>
        ///     The type in which updater works.
        /// </summary>
        /// <remarks>
        ///     TODO: implement thread safty of this property.
        ///     This property is freezable in case of working w
        /// </remarks>
        public UpdaterType Mode { get; set; }

        /// <summary>
        ///     Provides the default discovery to be loaded during <see cref="Mode"/> being set to <see cref="UpdaterType.Automatic"/>
        /// </summary>
        /// <remarks>
        ///     The default value are all currently loaded modules.
        /// </remarks>
        public IModuleDiscovery DefaultAfterUpdateModules
        {
            get
            {
                if (_defaultAfterUpdateModules == null)
                    return new DirectoryModuleDiscovery(_targetDirectory);

                return _defaultAfterUpdateModules;
            }
            set { _defaultAfterUpdateModules = value; }
        }


        /// <summary>
        /// Runs update checking. For each discovered module performs check for update. Uses <see cref="IDependencyChecker"/> to distinguish whether 
        /// the module needs to be udpated or not.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Method return result by <see cref="NomadAvailableUpdatesMessage"/> event, so it may be invoked asynchronously.
        /// </para>
        /// <para>
        ///     This methods does not throw any exception in case of failure, because the <see cref="Exception"/> derived classes
        /// cannot cross app domain boundaries. All information about failures are passed through <see cref="IEventAggregator"/> message bus.
        /// </para>
        /// </remarks>
        public void CheckUpdates(IModuleDiscovery moduleDiscovery)
        {
            Status = UpdaterStatus.Checking;

            AvailableModules availableModules;
            try
            {
                // connect to repository - fail safe
                availableModules = _modulesRepository.GetAvailableModules();
            }
            catch (Exception e)
            {
                Status = UpdaterStatus.Invalid;

                // publish information to modules about failing
                _eventAggregator.Publish(new NomadAvailableUpdatesMessage(
                                             new List<ModuleManifest>(), true, e.Message));
                return;
            }

            // null handling in repository if repository does not throw
            if (availableModules == null)
            {
                Status = UpdaterStatus.Invalid;
                _eventAggregator.Publish(new NomadAvailableUpdatesMessage(
                                             new List<ModuleManifest>(), true,
                                             "Null from repository"));
                return;
            }

            

            Status = UpdaterStatus.Checked;

            InvokeAvailableUpdates(new NomadAvailableUpdatesMessage(availableModules.Manifests));

            // if in automatic mode begin the download phase, use all the modules discovered as avaliable
            if (Mode == UpdaterType.Automatic)
                PrepareUpdate(new List<ModuleManifest>(availableModules.Manifests));
        }


        /// <summary>
        ///     Prepares update for available updates. Result returned as message.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Using provided <see cref="IModulesRepository"/> downloads all modules and their dependencies.
        /// </para>
        /// <para>
        ///     Method return result by <see cref="NomadUpdatesReadyMessage"/> event, so it may be invoked asynchronously.
        /// </para>
        /// <para>
        ///     This methods does not throw any exception in case of failure, because the <see cref="Exception"/> derived classes
        /// cannot cross app domain boundaries and wont invoke interrupt. All information about failures are passed through <see cref="IEventAggregator"/> message bus.
        /// </para>
        /// </remarks>
        /// <param name="availableUpdates">modules to install. </param>
        public void PrepareUpdate(IEnumerable<ModuleManifest> availableUpdates)
        {
            if (availableUpdates == null)
            {
                // can not throw exception - must change into message
                _eventAggregator.Publish(new NomadUpdatesReadyMessage(new List<ModuleManifest>(),
                                                                      true,
                                                                      "Argument cannot be null"));
                Status = UpdaterStatus.Invalid;
                return;
            }

            Status = UpdaterStatus.Preparing;

            // use dependency checker to get what is wanted
            // FIXME: the optimalization impact is here (mostly this linq is improper)

            IEnumerable<ModuleInfo> nonValidModules = null;
            IEnumerable<ModuleInfo> loadedModules = _modulesOperations.GetLoadedModules();
            var rnd = new Random();
            IEnumerable<ModuleInfo> newModules = availableUpdates.Select( x => new ModuleInfo(rnd.Next().ToString(),x,null)).ToList();

            if(!_dependencyChecker.CheckModules(loadedModules,newModules,out nonValidModules))
            {
                // publish information about not feasible depenencies
                _eventAggregator.Publish(
                    new NomadUpdatesReadyMessage(nonValidModules.Select(x => x.Manifest).ToList(),
                                                 true, "Dependencies could not be resolved"));

                Status = UpdaterStatus.Invalid;
                return;
            }

            var modulePackages = new Dictionary<string, ModulePackage>();
            try
            {
                // FIXME: dont get the packages which are already installed. 
                foreach (ModuleManifest availableUpdate in availableUpdates)
                {
                    foreach (ModuleDependency moduleDependency in availableUpdate.ModuleDependencies
                        )
                    {
                        // preventing getting the same file twice
                        if (!modulePackages.ContainsKey(moduleDependency.ModuleName))
                            modulePackages[moduleDependency.ModuleName] =
                                _modulesRepository.GetModule(moduleDependency.ModuleName);
                    }
                    // preventing getting the same file twice
                    if (!modulePackages.ContainsKey(availableUpdate.ModuleName))
                        modulePackages[availableUpdate.ModuleName] =
                            _modulesRepository.GetModule(availableUpdate.ModuleName);
                }
            }
            catch (Exception e)
            {
                // change exception into message
                _eventAggregator.Publish(new NomadUpdatesReadyMessage(new List<ModuleManifest>(),
                                                                      true, e.Message));
                Status = UpdaterStatus.Invalid;
                return;
            }

            Status = UpdaterStatus.Prepared;

            // verify the packages (simple verification - not checking zips) TODO: provide better verification ?
            if (modulePackages.Values.Any(modulePackage => modulePackage.ModuleManifest == null))
            {
                _eventAggregator.Publish(new NomadUpdatesReadyMessage(new List<ModuleManifest>(),
                                                                      true,
                                                                      "Null reference in ModuleManifest"));
                Status = UpdaterStatus.Invalid;
                return;
            }

            _modulesPackages = modulePackages.Values.ToList();

            InvokeUpdatePackagesReady(
                new NomadUpdatesReadyMessage(_modulesPackages.Select(x => x.ModuleManifest).ToList()));

            // if in automation mode use all packages to perform update, use default as ones to be loaded after
            if (Mode == UpdaterType.Automatic)
                PerformUpdates(DefaultAfterUpdateModules);
        }


        /// <summary>
        ///     Starts update process
        /// </summary>
        /// <remarks>
        /// <para>
        /// Using provided <see cref="IModulesOperations"/> it unloads all modules, 
        /// than it places update files into modules directory, and loads modules back.
        /// </para>
        /// 
        /// <para>
        /// Upon success or failure sets the flag <see cref="Status"/> with corresponding value. 
        /// </para>
        /// TODO: provide better documentation
        /// </remarks>
        public void PerformUpdates(IModuleDiscovery afterUpdateModulesToBeLoaded)
        {
            Status = UpdaterStatus.Performing;
            UpdateFinished = new AutoResetEvent(false);

            ThreadPool.QueueUserWorkItem(delegate
                                             {
                                                 IEnumerable<ModulePackage> modulePackages =
                                                     _modulesPackages;
                                                 try
                                                 {
                                                     _modulesOperations.UnloadModules();

                                                     foreach (
                                                         ModulePackage modulePackage in
                                                             modulePackages)
                                                     {
                                                         _modulePackager.PerformUpdates(
                                                             _targetDirectory, modulePackage);
                                                     }

                                                     _modulesOperations.LoadModules(
                                                         afterUpdateModulesToBeLoaded);
                                                 }
                                                 catch (Exception)
                                                 {
                                                     // catch exceptions, TODO: add logging for this
                                                     Status = UpdaterStatus.Invalid;
                                                     
                                                     UpdateFinished.Set();
                                                     UpdateFinished.Close();

                                                     return;
                                                 }

                                                 // set result of the updates
                                                 Status = UpdaterStatus.Idle;

                                                 // make signal about finishing the update.
                                                 UpdateFinished.Set();

                                                 // free the system resurces
                                                 UpdateFinished.Close();
                                             });
        }

        #endregion

        private void InvokeAvailableUpdates(NomadAvailableUpdatesMessage e)
        {
            _eventAggregator.Publish(e);
        }


        private void InvokeUpdatePackagesReady(NomadUpdatesReadyMessage e)
        {
            _eventAggregator.Publish(e);
        }
    }
}