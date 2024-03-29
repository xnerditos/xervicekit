using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.MetaServices;

namespace XKit.Lib.Host.MetaServices.RegistrationsManagement {

    public interface IRegistrationsManagementMetaServiceFactory {
        IRegistrationsManagementMetaService Create(
            IXKitHost xKitHost
        );
    }

    public class RegistrationsManagementMetaServiceFactory : IRegistrationsManagementMetaServiceFactory {

        private static IRegistrationsManagementMetaServiceFactory factory = new RegistrationsManagementMetaServiceFactory();

        public static IRegistrationsManagementMetaServiceFactory Factory => factory;

        // =====================================================================
        // IRegistrationManagementMetaServiceFactory
        // =====================================================================
        IRegistrationsManagementMetaService IRegistrationsManagementMetaServiceFactory.Create(
            IXKitHost xKitHost
        ) {
            return new RegistrationManagementMetaService(xKitHost);
        }

        // =====================================================================
        // Static 
        // ===================================================================== 

        public static IRegistrationsManagementMetaService Create(
            IXKitHost xKitHost
        ) => Factory.Create(xKitHost);

        public static void InjectCustomFactory(IRegistrationsManagementMetaServiceFactory factory)
            => RegistrationsManagementMetaServiceFactory.factory = factory;
    }
}
