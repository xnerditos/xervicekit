using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace TestServices.SvcWithGenericTimer {

	public interface ISvcWithGenericTimerServiceFactory : ITestServiceFactory {}

	public class SvcWithGenericTimerServiceFactory : ISvcWithGenericTimerServiceFactory {
		private static ISvcWithGenericTimerServiceFactory factory = new SvcWithGenericTimerServiceFactory();

		public static ISvcWithGenericTimerServiceFactory Factory => factory;

        // =====================================================================
        // IMockServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            IXkitHostEnvironment hostEnv
        ) {
            if (hostEnv == null) { throw new ArgumentNullException(nameof(hostEnv)); }
            return new SvcWithGenericTimerService(hostEnv);
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

        public static void InjectCustomFactory(ISvcWithGenericTimerServiceFactory factory) =>
            SvcWithGenericTimerServiceFactory.factory = factory; 
	}
}
