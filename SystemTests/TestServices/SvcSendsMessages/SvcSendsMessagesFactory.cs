using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;

namespace TestServices.SvcSendsMessages;

public interface ISvcSendsMessagesServiceFactory : ITestServiceFactory {}

public class SvcSendsMessagesServiceFactory : ISvcSendsMessagesServiceFactory {
    private static ISvcSendsMessagesServiceFactory factory = new SvcSendsMessagesServiceFactory();

    public static ISvcSendsMessagesServiceFactory Factory => factory;

    // =====================================================================
    // ISvcSimpleServiceFactory
    // =====================================================================

    IManagedService ITestServiceFactory.Create(
        IXKitHostEnvironment xkitEnvironment
    ) {
        if (xkitEnvironment == null) { throw new ArgumentNullException(nameof(xkitEnvironment)); }
        return new SvcSendsMessagesService(xkitEnvironment);
    } 

    IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;

    // =====================================================================
    // Static methods
    // =====================================================================

    public static IManagedService Create(
        IXKitHostEnvironment xkitEnvironment
    ) => Factory.Create(xkitEnvironment);

    public static void InjectCustomFactory(ISvcSendsMessagesServiceFactory factory) =>
        SvcSendsMessagesServiceFactory.factory = factory; 
}
