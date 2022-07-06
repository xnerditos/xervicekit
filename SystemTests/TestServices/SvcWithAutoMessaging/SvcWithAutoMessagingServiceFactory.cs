using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace TestServices.SvcWithAutoMessaging {

	public interface ISvcWithAutoMessagingServiceFactory : ITestServiceFactory {}

	public class SvcWithAutoMessagingServiceFactory : ISvcWithAutoMessagingServiceFactory {
		private static readonly ISvcWithAutoMessagingServiceFactory factory = new SvcWithAutoMessagingServiceFactory();

		public static ISvcWithAutoMessagingServiceFactory Factory => factory;

        // =====================================================================
        // IMockServiceFactory
        // =====================================================================

		IManagedService ITestServiceFactory.Create(
            IXKitHostEnvironment xkitEnvironment
        ) {
            if (xkitEnvironment == null) { throw new ArgumentNullException(nameof(xkitEnvironment)); }
            return new SvcWithAutoMessagingService(xkitEnvironment);
        } 

        // =====================================================================
        // IServiceFactory
        // =====================================================================

        IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;        

        // =====================================================================
        // Static
        // =====================================================================

        public static IManagedService Create(
            IXKitHostEnvironment xkitEnvironment
        ) => Factory.Create(xkitEnvironment);
	}
}
