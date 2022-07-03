using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Host;

namespace TestServices.SvcWithDependency1; 
    
public interface ISvcWithDependency1ClientFactory : IServiceClientFactory<ISvcWithDependency1> {	}

public class SvcWithDependency1ClientFactory : ISvcWithDependency1ClientFactory {
private static ISvcWithDependency1ClientFactory factory = new SvcWithDependency1ClientFactory();

    public static ISvcWithDependency1ClientFactory Factory => factory;

    // =====================================================================
    // IServiceClientFactory<IRegistryClient>
    // =====================================================================

    ISvcWithDependency1 IServiceClientFactory<ISvcWithDependency1>.CreateServiceClient(
        ILogSession log,
        IDependencyConnector connector,
        ServiceCallTypeParameters defaultCallTypeParameters
    ) => new SvcWithDependency1Client(
            Constants.ServiceDescriptor,
            log,
            connector,
            defaultCallTypeParameters
        );

    IReadOnlyDescriptor IServiceClientFactory.Descriptor => Constants.ServiceDescriptor;
    
    // =====================================================================
    // Static methods
    // =====================================================================

    public static void InjectCustomFactory(ISvcWithDependency1ClientFactory factory) =>
        SvcWithDependency1ClientFactory.factory = factory; 
}
