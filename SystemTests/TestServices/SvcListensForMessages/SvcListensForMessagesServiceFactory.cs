using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace TestServices.SvcListensForMessages; 

public interface ISvcListensForMessagesServiceFactory : ITestServiceFactory {}

public class SvcListensForMessagesServiceFactory : ISvcListensForMessagesServiceFactory {
    private static ISvcListensForMessagesServiceFactory factory = new SvcListensForMessagesServiceFactory();

    public static ISvcListensForMessagesServiceFactory Factory => factory;

    // =====================================================================
    // IMockServiceFactory
    // =====================================================================

    IManagedService ITestServiceFactory.Create(
        IXKitHostEnvironment xkitEnvironment
    ) {
        if (xkitEnvironment == null) { throw new ArgumentNullException(nameof(xkitEnvironment)); }
        return new SvcListensForMessagesService(xkitEnvironment);
    } 

    // =====================================================================
    // IServiceFactory
    // =====================================================================

    IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;
    
    // =====================================================================
    // Static methods
    // =====================================================================

    public static IManagedService Create(
        IXKitHostEnvironment xkitEnvironment
    ) => Factory.Create(xkitEnvironment);

    public static void InjectCustomFactory(ISvcListensForMessagesServiceFactory factory) =>
        SvcListensForMessagesServiceFactory.factory = factory; 
}
