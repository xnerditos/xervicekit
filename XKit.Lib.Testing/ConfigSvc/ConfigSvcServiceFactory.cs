using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace XKit.Lib.Testing.TestConfigSvc {

    public interface IConfigSvcServiceFactory : IServiceFactory {
		IManagedService Create(
            ILocalEnvironment localEnvironment
        );
    }

	public class ConfigSvcServiceFactory : IConfigSvcServiceFactory
	{
		private static IConfigSvcServiceFactory factory = new ConfigSvcServiceFactory();

		public static IConfigSvcServiceFactory Factory => factory;

        IReadOnlyDescriptor IServiceFactory.Descriptor => XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Config.Descriptor;

        // =====================================================================
        // IRegistrySvcServiceFactory
        // =====================================================================

		IManagedService IConfigSvcServiceFactory.Create(
            ILocalEnvironment localEnvironment
        ) {
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new ConfigSvcService(localEnvironment);
        } 

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment
        ) => Factory.Create(localEnvironment);

        public static void InjectCustomFactory(IConfigSvcServiceFactory factory) =>
            ConfigSvcServiceFactory.factory = factory; 
	}
}
