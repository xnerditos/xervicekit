using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcSendsMessages.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests.ServiceCalls.SvcSendsMessages {

    public interface ISvcSendsMessagesApi : IServiceApi {

        Task<ServiceCallResult> RaisesEvent1(Message request);
        Task<ServiceCallResult> RaisesEvent2(Message request);
        Task<ServiceCallResult> IssuesCommand1(Message request);
        Task<ServiceCallResult> IssuesCommand2(Message request);
        Task<ServiceCallResult> IssuesCommand1AndWaitsForFinish(Message request);

    }
}