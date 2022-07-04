using System.Collections.Generic;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace TestServices.SvcWithGenericTimer {
    
	public interface ISvcWithGenericTimerClientFactory : IServiceClientFactory<ISvcWithGenericTimerApi> {	}

	public class SvcWithGenericTimerClientFactory : ISvcWithGenericTimerClientFactory {
		private static ISvcWithGenericTimerClientFactory factory = new SvcWithGenericTimerClientFactory();

		public static ISvcWithGenericTimerClientFactory Factory => factory;

        // =====================================================================
        // IServiceClientFactory<IRegistryClient>
        // =====================================================================

		ISvcWithGenericTimerApi IServiceClientFactory<ISvcWithGenericTimerApi>.CreateServiceClient(
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters
		) => new SvcWithGenericTimerClient(
				Constants.ServiceDescriptor,
                log,
				connector,
                defaultCallTypeParameters
			);

		IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
		
        // =====================================================================
        // Static methods
        // =====================================================================

        public static void InjectCustomFactory(ISvcWithGenericTimerClientFactory factory) =>
            SvcWithGenericTimerClientFactory.factory = factory; 
	}
}
