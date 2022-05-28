using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Common.Services.MessageBroker {
    public interface IMessageBrokerApi : IServiceApi {

        Task<ServiceCallResult> RaiseEvent(FabricMessage request);

        Task<ServiceCallResult> IssueCommand(FabricMessage request);

        Task<ServiceCallResult<WaitOnMessageResponse>> WaitOnMessage(WaitOnMessageRequest request);

        Task<ServiceCallResult> Subscribe(SubscribeRequest request);
    }
}