using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace TestServices.SvcSendsMessages; 

public class SvcSendsMessagesClient : ServiceClientBase<ISvcSendsMessagesApi>, ISvcSendsMessagesApi {

    public SvcSendsMessagesClient(
        IReadOnlyDescriptor dependency,
        XKit.Lib.Common.Log.ILogSession log,
        IFabricConnector connector,
        ServiceCallTypeParameters defaultCallTypeParameters
    ) : base(
        dependency,
        log,
        connector,
        defaultCallTypeParameters
    ) { }

    // =====================================================================
    // overridables
    // =====================================================================

    protected override IReadOnlyDescriptor ServiceDescriptor => Constants.ServiceDescriptor;

    // =====================================================================
    // ISvcSimple
    // =====================================================================

    Task<ServiceCallResult> ISvcSendsMessagesApi.RaisesEvent1(
        Message request
    ) => ExecuteCall<Message>(request);

    Task<ServiceCallResult> ISvcSendsMessagesApi.RaisesEvent2(
        Message request
    ) => ExecuteCall<Message>(request);

    Task<ServiceCallResult> ISvcSendsMessagesApi.IssuesCommand1(
        Message request
    ) => ExecuteCall<Message>(request);

    Task<ServiceCallResult> ISvcSendsMessagesApi.IssuesCommand2(
        Message request
    ) => ExecuteCall<Message>(request);

    Task<ServiceCallResult> ISvcSendsMessagesApi.IssuesCommand1AndWaitsForFinish(
        Message request
    ) => ExecuteCall<Message>(request);
}

