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
        IXKitHostEnvironment xkitEnvironment
    ) {
        if (xkitEnvironment == null) { throw new ArgumentNullException(nameof(xkitEnvironment)); }
        return new SvcSimpleService(xkitEnvironment);
    }

    IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;

    // =====================================================================
    // Static methods
    // =====================================================================

    public static IManagedService Create(
        IXKitHostEnvironment xkitEnvironment
    ) => Factory.Create(xkitEnvironment);

    public static void InjectCustomFactory(ISvcSimpleServiceFactory factory) =>
        SvcSimpleServiceFactory.factory = factory;
}
