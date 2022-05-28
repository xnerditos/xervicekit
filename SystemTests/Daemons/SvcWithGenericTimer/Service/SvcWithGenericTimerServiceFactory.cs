using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.ObjectInstantiation;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace SystemTests.Daemons.SvcWithGenericTimer.Service {

	public interface ISvcWithGenericTimerServiceFactory : ITestServiceFactory {}

	public class SvcWithGenericTimerServiceFactory : ISvcWithGenericTimerServiceFactory {
		private static ISvcWithGenericTimerServiceFactory factory = new SvcWithGenericTimerServiceFactory();

		public static ISvcWithGenericTimerServiceFactory Factory => factory;

        // =====================================================================
        // IMockServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            ILocalEnvironment localEnvironment
        ) {
            localEnvironment ??= InProcessGlobalObjectRepositoryFactory.CreateSingleton().GetObject<ILocalEnvironment>(); 
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new SvcWithGenericTimerService(localEnvironment);
        } 

        // =====================================================================
        // IServiceFactory
        // =====================================================================

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;
        
        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment = null
        ) => SvcWithGenericTimerServiceFactory.Factory.Create(localEnvironment);

        public static void InjectCustomFactory(ISvcWithGenericTimerServiceFactory factory) =>
            SvcWithGenericTimerServiceFactory.factory = factory; 
	}
}
