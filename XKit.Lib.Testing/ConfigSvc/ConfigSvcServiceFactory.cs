using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace XKit.Lib.Testing.TestConfigSvc {

    public interface IConfigSvcServiceFactory : IServiceFactory {
		IManagedService Create(
            IXKitHostEnvironment hostEnv
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
            IXKitHostEnvironment hostEnv
        ) {
            if (hostEnv == null) { throw new ArgumentNullException(nameof(hostEnv)); }
            return new ConfigSvcService(hostEnv);
        } 

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            IXKitHostEnvironment hostEnv
        ) => Factory.Create(hostEnv);

        public static void InjectCustomFactory(IConfigSvcServiceFactory factory) =>
            ConfigSvcServiceFactory.factory = factory; 
	}
}
