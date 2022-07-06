using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace TestServices.SvcWithDependency1; 

public interface ISvcWithDependency1ServiceFactory : ITestServiceFactory {}

public class SvcWithDependency1ServiceFactory : ISvcWithDependency1ServiceFactory
{
    private static ISvcWithDependency1ServiceFactory factory = new SvcWithDependency1ServiceFactory();

    public static ISvcWithDependency1ServiceFactory Factory => factory;

    // =====================================================================
    // ISvcWithDependency1ServiceFactory
    // =====================================================================

    IManagedService ITestServiceFactory.Create(
        IXKitHostEnvironment xkitEnvironment
    ) {
        if (xkitEnvironment == null) { throw new ArgumentNullException(nameof(xkitEnvironment)); }
        return new SvcWithDependency1Service(xkitEnvironment);
    } 

    IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;

    // =====================================================================
    // Static methods
    // =====================================================================

    public static IManagedService Create(
        IXKitHostEnvironment xkitEnvironment
    ) => Factory.Create(xkitEnvironment);

    public static void InjectCustomFactory(ISvcWithDependency1ServiceFactory factory) =>
        SvcWithDependency1ServiceFactory.factory = factory; 
}
