using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace TestServices.SvcSimple; 

public interface ISvcSimpleServiceFactory : ITestServiceFactory { }

public class SvcSimpleServiceFactory : ISvcSimpleServiceFactory {
    private static ISvcSimpleServiceFactory factory = new SvcSimpleServiceFactory();

    public static ISvcSimpleServiceFactory Factory => factory;

    // =====================================================================
    // ISvcSimpleServiceFactory
    // =====================================================================

    IManagedService ITestServiceFactory.Create(
        ILocalEnvironment localEnvironment
    ) {
        if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
        return new SvcSimpleService(localEnvironment);
    }

    IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;

    // =====================================================================
    // Static methods
    // =====================================================================

    public static IManagedService Create(
        ILocalEnvironment localEnvironment
    ) => SvcSimpleServiceFactory.Factory.Create(localEnvironment);

    public static void InjectCustomFactory(ISvcSimpleServiceFactory factory) =>
        SvcSimpleServiceFactory.factory = factory;
}
