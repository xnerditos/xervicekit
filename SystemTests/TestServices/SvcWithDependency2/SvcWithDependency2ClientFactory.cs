using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Client;

namespace TestServices.SvcWithDependency2 {

    public interface ISvcWithDependency2ClientFactory : IServiceClientFactory<ISvcWithDependency2> {	}

	public class SvcWithDependency2ClientFactory : ISvcWithDependency2ClientFactory {
		private static ISvcWithDependency2ClientFactory factory = new SvcWithDependency2ClientFactory();

		public static ISvcWithDependency2ClientFactory Factory => factory;

        // =====================================================================
        // IServiceClientFactory<IRegistryClient>
        // =====================================================================

		ISvcWithDependency2 IServiceClientFactory<ISvcWithDependency2>.CreateServiceClient(
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters
		) => new SvcWithDependency2Client(
				Constants.ServiceDescriptor,
                log,
				connector,
                defaultCallTypeParameters
			);

		IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
		
        // =====================================================================
        // Static methods
        // =====================================================================

        public static ISvcWithDependency2 CreateServiceClient(
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null
        ) => Factory.CreateServiceClient(
            log,
            connector,
            defaultCallTypeParameters
		);

        public static void InjectCustomFactory(ISvcWithDependency2ClientFactory factory) =>
            SvcWithDependency2ClientFactory.factory = factory; 
	}
}
