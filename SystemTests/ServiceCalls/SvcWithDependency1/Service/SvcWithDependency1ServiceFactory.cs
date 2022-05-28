using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.ObjectInstantiation;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace SystemTests.ServiceCalls.SvcWithDependency1.Service {

	public interface ISvcWithDependency1ServiceFactory : ITestServiceFactory {}

	public class SvcWithDependency1ServiceFactory : ISvcWithDependency1ServiceFactory
	{
		private static ISvcWithDependency1ServiceFactory factory = new SvcWithDependency1ServiceFactory();

		public static ISvcWithDependency1ServiceFactory Factory => factory;

        // =====================================================================
        // ISvcWithDependency1ServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            ILocalEnvironment localEnvironment
        ) {
            localEnvironment ??= InProcessGlobalObjectRepositoryFactory.CreateSingleton().GetObject<ILocalEnvironment>(); 
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new SvcWithDependency1Service(localEnvironment);
        } 

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment = null
        ) => Factory.Create(localEnvironment);

        public static void InjectCustomFactory(ISvcWithDependency1ServiceFactory factory) =>
            SvcWithDependency1ServiceFactory.factory = factory; 
	}
}
