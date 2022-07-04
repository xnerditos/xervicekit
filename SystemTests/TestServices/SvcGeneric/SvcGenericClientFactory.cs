using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace TestServices.SvcGeneric; 
    
public interface ISvcGenericClientFactory : IServiceClientFactory<ISvcGenericApi> {	}

public class SvcGenericClientFactory : ISvcGenericClientFactory {
    private static ISvcGenericClientFactory factory = new SvcGenericClientFactory();

    public static ISvcGenericClientFactory Factory => factory;

    // =====================================================================
    // IServiceClientFactory<IRegistryClient>
    // =====================================================================

    ISvcGenericApi IServiceClientFactory<ISvcGenericApi>.CreateServiceClient(
        ILogSession log,
        IFabricConnector connector,
        ServiceCallTypeParameters defaultCallTypeParameters
    ) => new SvcGenericClient(
            Constants.ServiceDescriptor,
            log,
            connector,
            defaultCallTypeParameters
        );

    IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
    
    // =====================================================================
    // Static methods
    // =====================================================================

    public static void InjectCustomFactory(ISvcGenericClientFactory factory) =>
        SvcGenericClientFactory.factory = factory; 
}
