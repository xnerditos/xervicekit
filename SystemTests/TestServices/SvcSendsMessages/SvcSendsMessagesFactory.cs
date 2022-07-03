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
        ILocalEnvironment localEnvironment
    ) {
        if (localEnvironment == null) { throw new ArgumentNullException(nameof(localEnvironment)); }
        return new SvcSendsMessagesService(localEnvironment);
    } 

    IReadOnlyDescriptor IServiceFactory.Descriptor => Constants.ServiceDescriptor;

    // =====================================================================
    // Static methods
    // =====================================================================

    public static IManagedService Create(
        ILocalEnvironment localEnvironment
    ) => Factory.Create(localEnvironment);

    public static void InjectCustomFactory(ISvcSendsMessagesServiceFactory factory) =>
        SvcSendsMessagesServiceFactory.factory = factory; 
}
