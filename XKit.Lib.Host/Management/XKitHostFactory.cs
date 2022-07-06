using System;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Host.Helpers;
using XKit.Lib.Connector.Fabric;

namespace XKit.Lib.Host.Management {

    public interface IXkitHostFactory {
        IXKitHost Create(
            string hostAddress,
            string localMetaDataDbPath,
            string localDataFolderPath,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory,
            IFabricConnector connector,
            IConfigClient configClient = null,
            IMessageBrokerClient messagingClient = null
        );
        void SetHealthChecker(Func<HealthEnum> healthChecker);
    }

    public class XKitHostFactory : IXkitHostFactory {

        private static IXkitHostFactory factory = new XKitHostFactory(
            () => HealthEnum.Unknown
        );

        public static IXkitHostFactory Factory => factory;

        private Func<HealthEnum> healthChecker;

        public XKitHostFactory(Func<HealthEnum> defaultHealthChecker) {
            this.healthChecker = defaultHealthChecker;
        }

        // =====================================================================
        // IXkitHostFactory
        // =====================================================================

        IXKitHost IXkitHostFactory.Create(
            string hostAddress,
            string localMetaDataDbPath,
            string localDataFolderPath,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory,
            IFabricConnector connector,
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
            if (connector == null) {
                throw new ArgumentNullException("Must provide fabric connector");
            }
            
            var xKitHost = new XKitHost(
                hostAddress,
                connector,
                logSessionFactory,
                localConfigSessionFactory,
                localMetaDataDbPath,
                localDataFolderPath,
                healthChecker,
                configClient,
                messagingClient
            );

            return xKitHost;
        }

        void IXkitHostFactory.SetHealthChecker(Func<HealthEnum> healthChecker) {
            this.healthChecker = healthChecker;
        }

        // =====================================================================
        // Static
        // =====================================================================

        public static IXKitHost Create(
            string hostAddress,
            string localMetaDataDbPath,
            string localDataFolderPath,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory,
            IFabricConnector connector,
            IConfigClient configClient = null
        ) => Factory.Create(
            hostAddress,
            localMetaDataDbPath,
            localDataFolderPath,
            logSessionFactory,
            localConfigSessionFactory,
            connector,
            configClient
        );
        
        public static void InjectCustomFactory(IXkitHostFactory factory)
            => XKitHostFactory.factory = factory;

         public static void SetHealthChecker(Func<HealthEnum> healthChecker)
            => Factory.SetHealthChecker(healthChecker);
    }

    /*
     * For a health checker implementation, see https://github.com/dotnet-architecture/HealthChecks/tree/dev/Microsoft.Extensions.HealthChecks
     */
}
