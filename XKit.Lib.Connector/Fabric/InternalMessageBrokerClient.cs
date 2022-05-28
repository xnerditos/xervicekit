using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Services;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Connector.Service;

namespace XKit.Lib.Connector.Fabric {

    public interface IMessageBrokerClient : IServiceClient<IMessageBrokerApi>, IMessageBrokerApi { }

    public class InternalMessageBrokerClient : ServiceClientBase<IMessageBrokerApi>, IMessageBrokerClient {
        public InternalMessageBrokerClient(
            ILogSession log,
            IDependencyConnector connector,
            ServiceClientErrorHandling errorHandling = ServiceClientErrorHandling.LogError,
            ServiceCallTypeParameters callType = null
        ) : base(
            StandardConstants.Managed.StandardServices.MessageBroker.Descriptor,
            log,
            connector,
            callType ?? ServiceCallTypeParameters.SyncResult(),
            errorHandling,
            null
        ) { }

        Task<ServiceCallResult> IMessageBrokerApi.RaiseEvent(FabricMessage request)
            => ExecuteCall<FabricMessage>(request);
        
        Task<ServiceCallResult> IMessageBrokerApi.IssueCommand(FabricMessage request)
            => ExecuteCall<FabricMessage>(request);

        Task<ServiceCallResult<WaitOnMessageResponse>> IMessageBrokerApi.WaitOnMessage(WaitOnMessageRequest request)
            => ExecuteCall<WaitOnMessageRequest, WaitOnMessageResponse>(request);

        Task<ServiceCallResult> IMessageBrokerApi.Subscribe(SubscribeRequest request)
            => ExecuteCall<SubscribeRequest>(request);
    }
}
