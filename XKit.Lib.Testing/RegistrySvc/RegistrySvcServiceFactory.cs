using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace XKit.Lib.Testing.TestRegistrySvc {

    public interface IRegistrySvcServiceFactory : IServiceFactory {
        IManagedService Create(
            ILocalEnvironment localEnvironment
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
            ILocalEnvironment localEnvironment
        ) {
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new RegistrySvcService(localEnvironment);
        }

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment
        ) => Factory.Create(localEnvironment);

        public static void InjectCustomFactory(IRegistrySvcServiceFactory factory) =>
            RegistrySvcServiceFactory.factory = factory;
    }
}
