using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace XKit.Lib.Testing.TestMessageBrokerSvc {

    public interface IMessageBrokerSvcServiceFactory : IServiceFactory {
		IManagedService Create(
            IXKitHostEnvironment hostEnv
        );
    }

	public class MessageBrokerSvcServiceFactory : IMessageBrokerSvcServiceFactory
	{
		private static IMessageBrokerSvcServiceFactory factory = new MessageBrokerSvcServiceFactory();

		public static IMessageBrokerSvcServiceFactory Factory => factory;

        IReadOnlyDescriptor IServiceFactory.Descriptor => XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.MessageBroker.Descriptor;

        // =====================================================================
        // IRegistrySvcServiceFactory
        // =====================================================================

		IManagedService IMessageBrokerSvcServiceFactory.Create(
            IXKitHostEnvironment hostEnvironment
        ) {
            if (hostEnvironment == null) { throw new ArgumentNullException(nameof(hostEnvironment)); }
            return new MessageBrokerSvcService(hostEnvironment);
        } 

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IManagedService Create(
            IXKitHostEnvironment hostEnv
        ) => Factory.Create(hostEnv);

        public static void InjectCustomFactory(IMessageBrokerSvcServiceFactory factory) =>
            MessageBrokerSvcServiceFactory.factory = factory; 
	}
}
