using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.ObjectInstantiation;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace SystemTests.ServiceCalls.SvcWithDependency2.Service {

	public interface ISvcWithDependency2ServiceFactory : ITestServiceFactory { }

	public class SvcWithDependency2ServiceFactory : ISvcWithDependency2ServiceFactory
	{
		private static ISvcWithDependency2ServiceFactory factory = new SvcWithDependency2ServiceFactory();

		public static ISvcWithDependency2ServiceFactory Factory => factory;

        // =====================================================================
        // ISvcWithDependency2ServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            ILocalEnvironment localEnvironment
        ) {
            localEnvironment ??= InProcessGlobalObjectRepositoryFactory.CreateSingleton().GetObject<ILocalEnvironment>(); 
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new SvcWithDependency2Service(localEnvironment);
        } 

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;
        
        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment = null
        ) => Factory.Create(localEnvironment);

        public static void InjectCustomFactory(ISvcWithDependency2ServiceFactory factory) =>
            SvcWithDependency2ServiceFactory.factory = factory; 
	}
}
