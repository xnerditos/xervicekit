using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace SystemTests.ServiceCalls.SvcSimple.Client {

    public interface ISvcSimpleClientFactory : IServiceClientFactory<ISvcSimpleApi> {	}

	public class SvcSimpleClientFactory : ISvcSimpleClientFactory {
		private static ISvcSimpleClientFactory factory = new SvcSimpleClientFactory();

		public static ISvcSimpleClientFactory Factory => factory;

        // =====================================================================
        // IServiceClientFactory<IRegistryClient>
        // =====================================================================

		ISvcSimpleApi IServiceClientFactory<ISvcSimpleApi>.CreateServiceClient(
            ILogSession log,
            IDependencyConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters
		) => new SvcSimpleClient(
				Constants.ServiceDescriptor,
                log,
				connector,
                defaultCallTypeParameters
			);

		IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
		
        // =====================================================================
        // Static methods
        // =====================================================================

        public static ISvcSimpleApi CreateServiceClient(
            ILogSession log,
            IDependencyConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null
        ) => SvcSimpleClientFactory.Factory.CreateServiceClient(
            log,
            connector,
            defaultCallTypeParameters
		);

        public static void InjectCustomFactory(ISvcSimpleClientFactory factory) =>
            SvcSimpleClientFactory.factory = factory; 
	}
}
