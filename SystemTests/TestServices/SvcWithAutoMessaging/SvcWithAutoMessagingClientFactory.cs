using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace TestServices.SvcWithAutoMessaging {

    public interface ISvcWithAutoMessagingClientFactory : IServiceClientFactory<ISvcWithAutoMessagingApi> {	}

	public class SvcWithAutoMessagingClientFactory : ISvcWithAutoMessagingClientFactory {
		private static ISvcWithAutoMessagingClientFactory factory = new SvcWithAutoMessagingClientFactory();

		public static ISvcWithAutoMessagingClientFactory Factory => factory;

        // =====================================================================
        // IServiceClientFactory<IRegistryClient>
        // =====================================================================

		ISvcWithAutoMessagingApi IServiceClientFactory<ISvcWithAutoMessagingApi>.CreateServiceClient(
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters
		) => new SvcWithAutoMessagingClient(
				Constants.ServiceDescriptor,
                log,
				connector,
                defaultCallTypeParameters
			);

		IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
		
        // =====================================================================
        // Static methods
        // =====================================================================

        public static void InjectCustomFactory(ISvcWithAutoMessagingClientFactory factory) =>
            SvcWithAutoMessagingClientFactory.factory = factory; 
	}
}
