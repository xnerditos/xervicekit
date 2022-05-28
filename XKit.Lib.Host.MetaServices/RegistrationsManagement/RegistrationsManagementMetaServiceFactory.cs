using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.MetaServices;

namespace XKit.Lib.Host.MetaServices.RegistrationsManagement {

    public interface IRegistrationsManagementMetaServiceFactory {
        IRegistrationsManagementMetaService Create(
            IHostManager hostManager
        );
    }

    public class RegistrationsManagementMetaServiceFactory : IRegistrationsManagementMetaServiceFactory {

        private static IRegistrationsManagementMetaServiceFactory factory = new RegistrationsManagementMetaServiceFactory();

        public static IRegistrationsManagementMetaServiceFactory Factory => factory;

        // =====================================================================
        // IRegistrationManagementMetaServiceFactory
        // =====================================================================
        IRegistrationsManagementMetaService IRegistrationsManagementMetaServiceFactory.Create(
            IHostManager hostManager
        ) {
            return new RegistrationManagementMetaService(hostManager);
        }

        // =====================================================================
        // Static 
        // ===================================================================== 

        public static IRegistrationsManagementMetaService Create(
            IHostManager hostManager
        ) => Factory.Create(hostManager);

        public static void InjectCustomFactory(IRegistrationsManagementMetaServiceFactory factory)
            => RegistrationsManagementMetaServiceFactory.factory = factory;
    }
}
