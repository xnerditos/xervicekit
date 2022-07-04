using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace SystemTests._NAMESPACE.Svc1.Service {

	public interface ISvc1ServiceFactory : ITestServiceFactory {}

	public class Svc1ServiceFactory : ISvc1ServiceFactory {
		private static ISvc1ServiceFactory factory = new Svc1ServiceFactory();

		public static ISvc1ServiceFactory Factory => factory;

        // =====================================================================
        // IMockServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            IXkitHostEnvironment hostEnv
        ) {
            if (hostEnv == null) { throw new ArgumentNullException(nameof(hostEnv)); }
            return new Svc1Service(hostEnv);
        } 

        // =====================================================================
        // IServiceFactory
        // =====================================================================

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;
        
        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            IXkitHostEnvironment hostEnv
        ) => Factory.Create(hostEnv);

        public static void InjectCustomFactory(ISvc1ServiceFactory factory) =>
            Svc1ServiceFactory.factory = factory; 
	}
}
