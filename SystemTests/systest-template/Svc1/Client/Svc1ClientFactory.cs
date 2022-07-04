using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace SystemTests._NAMESPACE.Svc1.Client {

    public interface ISvc1ClientFactory : IServiceClientFactory<ISvc1Api> {	}

	public class Svc1ClientFactory : ISvc1ClientFactory {
		private static ISvc1ClientFactory factory = new Svc1ClientFactory();

		public static ISvc1ClientFactory Factory => factory;

        // =====================================================================
        // IServiceClientFactory<IRegistryClient>
        // =====================================================================

		ISvc1Api IServiceClientFactory<ISvc1Api>.CreateServiceClient(
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters
		) => new Svc1Client(
				Constants.ServiceDescriptor,
                log,
                connector,
                defaultCallTypeParameters
			);

		IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
		
        // =====================================================================
        // Static methods
        // =====================================================================

        public static ISvc1Api CreateServiceClient(
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null
        ) => Factory.CreateServiceClient(
            log,
            connector,
            defaultCallTypeParameters
		);

        public static void InjectCustomFactory(ISvc1ClientFactory factory) =>
            Svc1ClientFactory.factory = factory; 
	}
}
