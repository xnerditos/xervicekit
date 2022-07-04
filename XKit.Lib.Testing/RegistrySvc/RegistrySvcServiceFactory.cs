using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace XKit.Lib.Testing.TestRegistrySvc {

    public interface IRegistrySvcServiceFactory : IServiceFactory {
        IManagedService Create(
            IXkitHostEnvironment hostEnvironment
        );
    }

    public class RegistrySvcServiceFactory : IRegistrySvcServiceFactory {
        private static IRegistrySvcServiceFactory factory = new RegistrySvcServiceFactory();

        public static IRegistrySvcServiceFactory Factory => factory;

        IReadOnlyDescriptor IServiceFactory.Descriptor => XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Registry.Descriptor;

        // =====================================================================
        // IMockServiceFactory
        // =====================================================================

        IManagedService IRegistrySvcServiceFactory.Create(
            IXkitHostEnvironment hostEnv
        ) {
            if (hostEnv == null) { throw new ArgumentNullException(nameof(hostEnv)); }
            return new RegistrySvcService(hostEnv);
        }

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            IXkitHostEnvironment hostEnv
        ) => Factory.Create(hostEnv);

        public static void InjectCustomFactory(IRegistrySvcServiceFactory factory) =>
            RegistrySvcServiceFactory.factory = factory;
    }
}
