using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace TestServices.SvcSendsMessages; 
    
public interface ISvcSendsMessagesClientFactory : IServiceClientFactory<ISvcSendsMessagesApi> {	}

public class SvcSendsMessagesClientFactory : ISvcSendsMessagesClientFactory {
    private static ISvcSendsMessagesClientFactory factory = new SvcSendsMessagesClientFactory();

    public static ISvcSendsMessagesClientFactory Factory => factory;

    // =====================================================================
    // IServiceClientFactory<IRegistryClient>
    // =====================================================================

    ISvcSendsMessagesApi IServiceClientFactory<ISvcSendsMessagesApi>.CreateServiceClient(
        ILogSession log,
        IFabricConnector connector,
        ServiceCallTypeParameters defaultCallTypeParameters
    ) => new SvcSendsMessagesClient(
            Constants.ServiceDescriptor,
            log,
            connector,
            defaultCallTypeParameters
        );

    IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
    
    // =====================================================================
    // Static methods
    // =====================================================================

    public static ISvcSendsMessagesApi CreateServiceClient(
        ILogSession log,
        IFabricConnector connector,
        ServiceCallTypeParameters defaultCallTypeParameters = null
    ) => Factory.CreateServiceClient(
        log,
        connector,
        defaultCallTypeParameters
    );

    public static void InjectCustomFactory(ISvcSendsMessagesClientFactory factory) =>
        SvcSendsMessagesClientFactory.factory = factory; 
}
