using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace SystemTests.Daemons.SvcWithAutoMessaging.Service {

	public interface ISvcWithAutoMessagingServiceFactory : ITestServiceFactory {}

	public class SvcWithAutoMessagingServiceFactory : ISvcWithAutoMessagingServiceFactory {
		private static readonly ISvcWithAutoMessagingServiceFactory factory = new SvcWithAutoMessagingServiceFactory();

		public static ISvcWithAutoMessagingServiceFactory Factory => factory;

        // =====================================================================
        // IMockServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            ILocalEnvironment localEnvironment
        ) {
            if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
            return new SvcWithAutoMessagingService(localEnvironment);
        } 

        // =====================================================================
        // IServiceFactory
        // =====================================================================

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;        

        // =====================================================================
        // Static
        // =====================================================================

        public static IManagedService Create(
            ILocalEnvironment localEnvironment
        ) => Factory.Create(localEnvironment);
	}
}
