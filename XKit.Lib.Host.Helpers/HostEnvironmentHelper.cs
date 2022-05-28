using System;
using System.Collections.Generic;
using System.Linq;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.MetaServices;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.Management;
using XKit.Lib.Host.Config;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.ObjectInstantiation;
using XKit.Lib.Common.Fabric;
using System.IO;
using static System.Environment;
using XKit.Lib.Host.MetaServices.RegistrationsManagement;
using XKit.Lib.Connector.Fabric;
using XKit.Lib.Common;
using XKit.Lib.Common.Client;
using XKit.Lib.Connector.Protocols.Direct;
using XKit.Lib.Connector.Protocols.Http;

namespace XKit.Lib.Host {

    public static class HostEnvironmentHelper {
        
        private static readonly Lazy<IInProcessGlobalObjectRepository> igorLazy = new(() => InProcessGlobalObjectRepositoryFactory.CreateSingleton());
        private static IHostManager hostManager;
        private static IDependencyConnector dependencyConnector;
        private static IFabricConnector fabricConnector;
        private static ILogSessionFactory logSessionFactory;
        public static IInProcessGlobalObjectRepository InjectableGlobalObjectRepository => igorLazy.Value;
        public static IHostManager Host => hostManager; 
        public static IFabricConnector FabricConnector => fabricConnector; 
        public static IDependencyConnector DependencyConnector => dependencyConnector;
        public static ILogSessionFactory LogSessionFactory => logSessionFactory;
        public static IHostManager CreateInitHost(
            IList<IInstanceClientFactory> instanceClientFactories,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory = null,
            string hostAddress = null, 
            string localMetadataDbPath = null,
            string localDataStorageFolderPath = null,
            string localConfigFolderPath = null,
            Func<HealthEnum> healthChecker = null,
            string[] capabilitiesToRegister = null
        ) {
            if (InjectableGlobalObjectRepository.HasObject(typeof(IHostManager))) {
                throw new Exception("Host already created");
            }
            if (string.IsNullOrEmpty(hostAddress)) {
                hostAddress = Environment.GetEnvironmentVariable(EnvironmentHelperConstants.EnvironmentVariables.HostBaseAddress);
                if (string.IsNullOrEmpty(hostAddress)) {
                    throw new ArgumentException($"hostAddress must be passed or else {EnvironmentHelperConstants.EnvironmentVariables.HostBaseAddress} env variable must be defined.");
                }
            }

            instanceClientFactories ??= new[] { 
                DirectLocalClientFactory.Factory,
                HttpClientFactory.Factory
            };

            logSessionFactory ??= LocalLog.LogSessionFactory.Factory;

            HostEnvironmentHelper.logSessionFactory = logSessionFactory;
            
            localDataStorageFolderPath = EnsureLocalDataStoragePath(localDataStorageFolderPath);
            localMetadataDbPath = EnsurePath(
                path: localMetadataDbPath,
                defaultParentFolderPath: localDataStorageFolderPath,
                defaultName: "_metadata.db",
                environmentVariable: EnvironmentHelperConstants.EnvironmentVariables.MetaDbPath,
                isFolder: false
            );
            localConfigFolderPath = EnsurePath(
                path: localConfigFolderPath,
                defaultParentFolderPath: localDataStorageFolderPath,
                defaultName: "config",
                environmentVariable: EnvironmentHelperConstants.EnvironmentVariables.ConfigFolderPath,
                isFolder: true
            );

            if (localConfigSessionFactory == null) {
                localConfigSessionFactory = LocalConfigSessionFactory.Factory;
            }
            localConfigSessionFactory.SetPath(
                localConfigFolderPath
            );

            fabricConnector = FabricConnectorFactory.Create(instanceClientFactories);
            dependencyConnector = fabricConnector;

            HostManagerFactory.SetHealthChecker(healthChecker);

            hostManager = HostManagerFactory.Create(
                hostAddress,
                localMetadataDbPath,
                localDataStorageFolderPath,
                logSessionFactory,
                localConfigSessionFactory,
                fabricConnector
            );

            fabricConnector.Initialize();

            InjectableGlobalObjectRepository.RegisterSingleton(
                logSessionFactory, 
                typeof(ILogSessionFactory)
            );
            InjectableGlobalObjectRepository.RegisterSingleton(
                fabricConnector,
                typeof(IFabricConnector),
                typeof(IDependencyConnector)
            );
            InjectableGlobalObjectRepository.RegisterSingleton(
                hostManager, 
                typeof(IHostManager),
                typeof(IHostEnvironment),
                typeof(ILocalEnvironment)
            );
            
            bool doAll = capabilitiesToRegister == null;

            if (doAll || capabilitiesToRegister.Contains(StandardCapabilityNames.LocalRegistrationsManagement)) {
                hostManager.AddMetaService(
                    RegistrationsManagementMetaServiceFactory.Create(hostManager)
                );
            }

            return hostManager;
        }

        public static void StartHost(
            IEnumerable<string> initialRegistryAddresses = null,
            IDictionary<string, object> startupParameters = null, 
            bool failIfCannotRegister = false
        ) {
            if (hostManager != null) {

                if (initialRegistryAddresses == null) {
                    string registryAddresses = Environment.GetEnvironmentVariable(EnvironmentHelperConstants.EnvironmentVariables.InitialRegistryAddresses);
                    initialRegistryAddresses = registryAddresses?.Split(';', StringSplitOptions.RemoveEmptyEntries);
                }
                hostManager.StartHost(
                    initialRegistryAddresses ?? Array.Empty<string>(),
                    startupParameters,
                    failIfCannotRegister
                );
            }
        }        

        public static void PauseHost() {
            hostManager?.PauseHost();
        }        

        public static void ResumeHost() {
            hostManager?.ResumeHost();
        }        

        public static void StopAndDestroyHost() {
            hostManager.StopHost();
            InjectableGlobalObjectRepository.Clear();
            hostManager = null;
            logSessionFactory = null;
            dependencyConnector = null;            
        }        

        // =====================================================================
        // private 
        // =====================================================================

        // private static void RunWithDefaultMonitor(Action action) 
        //     => RunWithDefaultMonitor((logSession) => {
        //         action();
        //         return Task.CompletedTask;
        //     });

        // private static void RunWithDefaultMonitor(Action<ILogSession> action) 
        //     => RunWithDefaultMonitor((logSession) => {
        //         action(logSession);
        //         return Task.CompletedTask;
        //     });

        // private static void RunWithDefaultMonitor(Func<Task> action) 
        //     => RunWithDefaultMonitor((logSession) => action());
        
        // private static void RunWithDefaultMonitor(Func<ILogSession, Task> action) {

        //     TaskUtil.RunSyncSafely(async () => {

        //         if (logManager == null) { 
        //             await action(RuntimeMonitorFactory.Create(null), null);
        //         } else {
        //             using var logSession = logManager.CreateWriteableSession(
        //                 LogContextTypeEnum.HostAction,
        //                 Identifiers.NameOriginatorAsHost,
        //                 hostManager.VersionLevel.GetValueOrDefault(),
        //                 null,
        //                 hostManager.FabricId
        //             );

        //             await logSession.BeginLog();
        //             await action(logSession);
        //             await logSession.EndLog();
        //         }
        //     });
        // }
        
        private static string EnsureLocalDataStoragePath(string localDataStorageFolderPath) {
            return EnsurePath(
                localDataStorageFolderPath,
                Environment.GetFolderPath(SpecialFolder.ApplicationData),
                "xkit-host",
                EnvironmentHelperConstants.EnvironmentVariables.LocalDataFolderPath,
                isFolder: true
            );
        }

        private static string EnsurePath(
            string path, 
            string defaultParentFolderPath, 
            string defaultName, 
            string environmentVariable,
            bool isFolder
        ) {
            if (string.IsNullOrEmpty(path)) {
                path = System.Environment.GetEnvironmentVariable(environmentVariable);
                if (string.IsNullOrEmpty(path)) {
                    path = $"{defaultParentFolderPath}/{defaultName}";
                }
            }
            if (isFolder) {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
            }
            return path;
        }
    }
}
