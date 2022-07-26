using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Connector.Service;

namespace Samples.MessageBroker.Client; 
public interface IMessageBrokerClient : IMessageBrokerApi, IServiceClient<IMessageBrokerApi> { }

public class MessageBrokerClient : ServiceClientBase<IMessageBrokerApi>, IMessageBrokerClient {

    public MessageBrokerClient(
        ILogSession log, IFabricConnector connector, ServiceCallTypeParameters defaultCallTypeParameters = null
    ) : base(
        Constants.ServiceDescriptor,
        log,
        connector,
        defaultCallTypeParameters
    ) { }

    Task<ServiceCallResult> IMessageBrokerApi.IssueCommand(
        FabricMessage request
    ) => ExecuteCall(request);

    Task<ServiceCallResult> IMessageBrokerApi.RaiseEvent(
        FabricMessage request
    ) => ExecuteCall(request);

    Task<ServiceCallResult> IMessageBrokerApi.Subscribe(
        SubscribeRequest request
    ) => ExecuteCall(request);

    Task<ServiceCallResult> IMessageBrokerApi.Unsubscribe(
        UnsubscribeRequest request
    ) => ExecuteCall(request);

    Task<ServiceCallResult<WaitOnMessageResponse>> IMessageBrokerApi.WaitOnMessage(
        WaitOnMessageRequest request
    ) => ExecuteCall<WaitOnMessageRequest, WaitOnMessageResponse>(request);
}
