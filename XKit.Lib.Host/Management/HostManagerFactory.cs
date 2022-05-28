using System;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Host.Helpers;
using XKit.Lib.Connector.Fabric;

namespace XKit.Lib.Host.Management {

    public interface IHostManagerFactory {
        IHostManager Create(
            string hostAddress,
            string localMetaDataDbPath,
            string localDataFolderPath,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory,
            IFabricConnector fabricConnector,
            IConfigClient configClient = null,
            IMessageBrokerClient messagingClient = null
        );
        void SetHealthChecker(Func<HealthEnum> healthChecker);
    }

    public class HostManagerFactory : IHostManagerFactory {

        private static IHostManagerFactory factory = new HostManagerFactory(
            () => HealthEnum.Unknown
        );

        public static IHostManagerFactory Factory => factory;

        private Func<HealthEnum> healthChecker;

        public HostManagerFactory(Func<HealthEnum> defaultHealthChecker) {
            this.healthChecker = defaultHealthChecker;
        }

        // =====================================================================
        // IHostManagerFactory
        // =====================================================================

        IHostManager IHostManagerFactory.Create(
            string hostAddress,
            string localMetaDataDbPath,
            string localDataFolderPath,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory,
            IFabricConnector fabricConnector,
            IConfigClient configClient,
            IMessageBrokerClient messagingClient
        )  {
            if (string.IsNullOrEmpty(hostAddress)) {
                throw new ArgumentNullException(paramName: nameof(hostAddress));
            }
            if (logSessionFactory == null) {
                throw new ArgumentNullException("Must provide log manager");
            }
            if (localConfigSessionFactory == null) {
                throw new ArgumentNullException("Must provide the config manager factory");
            }                    
            if (string.IsNullOrEmpty(localMetaDataDbPath)) {
                throw new ArgumentNullException("Must provide the local meta data db path");
            }
            if (string.IsNullOrEmpty(localDataFolderPath)) {
                throw new ArgumentNullException("Must provide the local data folder");
            }
            if (fabricConnector == null) {
                throw new ArgumentNullException("Must provide fabric connector");
            }
            
            var hostManager = new HostManager(
                hostAddress,
                fabricConnector,
                logSessionFactory,
                localConfigSessionFactory,
                localMetaDataDbPath,
                localDataFolderPath,
                healthChecker,
                configClient,
                messagingClient
            );

            return hostManager;
        }

        void IHostManagerFactory.SetHealthChecker(Func<HealthEnum> healthChecker) {
            this.healthChecker = healthChecker;
        }

        // =====================================================================
        // Static
        // =====================================================================

        public static IHostManager Create(
            string hostAddress,
            string localMetaDataDbPath,
            string localDataFolderPath,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory,
            IFabricConnector fabricConnector,
            IConfigClient configClient = null
        ) => Factory.Create(
            hostAddress,
            localMetaDataDbPath,
            localDataFolderPath,
            logSessionFactory,
            localConfigSessionFactory,
            fabricConnector,
            configClient
        );
        
        public static void InjectCustomFactory(IHostManagerFactory factory)
            => HostManagerFactory.factory = factory;

         public static void SetHealthChecker(Func<HealthEnum> healthChecker)
            => Factory.SetHealthChecker(healthChecker);
    }

    /*
     * For a health checker implementation, see https://github.com/dotnet-architecture/HealthChecks/tree/dev/Microsoft.Extensions.HealthChecks
     */
}
