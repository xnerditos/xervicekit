using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace TestServices.SvcSendsMessages;

public interface ISvcSendsMessagesApi : IServiceApi {

    Task<ServiceCallResult> RaisesEvent1(Message request);
    Task<ServiceCallResult> RaisesEvent2(Message request);
    Task<ServiceCallResult> IssuesCommand1(Message request);
    Task<ServiceCallResult> IssuesCommand2(Message request);
    Task<ServiceCallResult> IssuesCommand1AndWaitsForFinish(Message request);

}
