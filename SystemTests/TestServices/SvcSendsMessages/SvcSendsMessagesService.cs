using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Utility;

namespace TestServices.SvcSendsMessages;

public class SvcSendsMessagesService 
    : ManagedService<SvcSendsMessagesOperation>, ISvcSendsMessagesService {

    private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
    
    // =====================================================================
    // overrides
    // =====================================================================

    protected override IReadOnlyDescriptor Descriptor => descriptor;

    protected override string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(descriptor);

    // =====================================================================
    // construction
    // =====================================================================

    public SvcSendsMessagesService(
        ILocalEnvironment localEnvironment
    ) : base(localEnvironment) { }
}
