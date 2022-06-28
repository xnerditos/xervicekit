using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace SystemTests.ServiceCalls.SvcListensForMessages.Service {

	public interface ISvcListensForMessagesServiceFactory : ITestServiceFactory {}

	public class SvcListensForMessagesServiceFactory : ISvcListensForMessagesServiceFactory {
		private static ISvcListensForMessagesServiceFactory factory = new SvcListensForMessagesServiceFactory();

		public static ISvcListensForMessagesServiceFactory Factory => factory;

        // =====================================================================
        // IMockServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            ILocalEnvironment localEnvironment
        ) {
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new SvcListensForMessagesService(localEnvironment);
        } 

        // =====================================================================
        // IServiceFactory
        // =====================================================================

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;
        
        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment
        ) => Factory.Create(localEnvironment);

        public static void InjectCustomFactory(ISvcListensForMessagesServiceFactory factory) =>
            SvcListensForMessagesServiceFactory.factory = factory; 
	}
}
