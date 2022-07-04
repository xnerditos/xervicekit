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

    public class HostEnvironmentHelper {
        
        private IXkitHost xKitHost;
        private IFabricConnector connector;
        private ILogSessionFactory logSessionFactory;
        public IXkitHost Host => xKitHost; 
        public IFabricConnector Connector => connector; 
        public ILogSessionFactory LogSessionFactory => logSessionFactory;
        public IXkitHost CreateInitHost(
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

            logSessionFactory ??= Log.LogSessionFactory.Factory;

            this.logSessionFactory = logSessionFactory;
            
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

            connector = FabricConnectorFactory.Create(instanceClientFactories);

            XkitHostFactory.SetHealthChecker(healthChecker);

            xKitHost = XkitHostFactory.Create(
                hostAddress,
                localMetadataDbPath,
                localDataStorageFolderPath,
                logSessionFactory,
                localConfigSessionFactory,
                connector
            );

            connector.Initialize();

            bool doAll = capabilitiesToRegister == null;

            if (doAll || capabilitiesToRegister.Contains(StandardCapabilityNames.LocalRegistrationsManagement)) {
                xKitHost.AddMetaService(
                    RegistrationsManagementMetaServiceFactory.Create(xKitHost)
                );
            }

            return xKitHost;
        }

        public void StartHost(
            IEnumerable<string> initialRegistryAddresses = null,
            IDictionary<string, object> startupParameters = null, 
            bool failIfCannotRegister = false
        ) {
            if (xKitHost != null) {

                if (initialRegistryAddresses == null) {
                    string registryAddresses = Environment.GetEnvironmentVariable(EnvironmentHelperConstants.EnvironmentVariables.InitialRegistryAddresses);
                    initialRegistryAddresses = registryAddresses?.Split(';', StringSplitOptions.RemoveEmptyEntries);
                }
                xKitHost.StartHost(
                    initialRegistryAddresses ?? Array.Empty<string>(),
                    startupParameters,
                    failIfCannotRegister
                );
            }
        }        

        public void PauseHost() {
            xKitHost?.PauseHost();
        }        

        public void ResumeHost() {
            xKitHost?.ResumeHost();
        }        

        public void StopAndDestroyHost() {
            xKitHost.StopHost();
            xKitHost = null;
            logSessionFactory = null;
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
        //                 xKitHost.VersionLevel.GetValueOrDefault(),
        //                 null,
        //                 xKitHost.FabricId
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
