using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.ObjectInstantiation;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace SystemTests.ServiceCalls.SvcSendsMessages.Service {

	public interface ISvcSendsMessagesServiceFactory : ITestServiceFactory {}

	public class SvcSendsMessagesServiceFactory : ISvcSendsMessagesServiceFactory {
		private static ISvcSendsMessagesServiceFactory factory = new SvcSendsMessagesServiceFactory();

		public static ISvcSendsMessagesServiceFactory Factory => factory;

        // =====================================================================
        // ISvcSimpleServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            ILocalEnvironment localEnvironment
        ) {
            localEnvironment ??= InProcessGlobalObjectRepositoryFactory.CreateSingleton().GetObject<ILocalEnvironment>(); 
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new SvcSendsMessagesService(localEnvironment);
        } 

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment = null
        ) => Factory.Create(localEnvironment);

        public static void InjectCustomFactory(ISvcSendsMessagesServiceFactory factory) =>
            SvcSendsMessagesServiceFactory.factory = factory; 
	}
}
