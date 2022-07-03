using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;

namespace TestServices.SvcListensForMessages; 

public interface ISvcListensForMessagesService : IManagedService, IServiceBase { 
    string TestValue { get; set; }
    string MessageName { get; set; } 
}

public class SvcListensForMessagesService 
    : ManagedService<SvcListensForMessagesOperation>, ISvcListensForMessagesService {

    private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
    
    protected override IReadOnlyDescriptor Descriptor => descriptor;

    public string TestValue { get; set; }
    public string MessageName { get; set; }

    // =====================================================================
    // construction
    // =====================================================================

    public SvcListensForMessagesService(
        ILocalEnvironment localEnvironment
    ) : base(localEnvironment) { 
        AddEventSubscription<SvcSendsMessages.TestEvents>(x => x.Event1(null));
        AddCommandSubscription<SvcSendsMessages.TestCommands>(x => x.Command1(null));
        AddCommandSubscription<SvcSendsMessages.TestCommands>(x => x.Command2(null));
    }
}
