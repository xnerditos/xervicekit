using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Testing.TestMessageBrokerSvc {

    public partial class MessageBrokerSvcOperation : ServiceOperation<IMessageBrokerSvcService>, IMessageBrokerApi {

        public MessageBrokerSvcOperation(
            ServiceOperationContext context
        ) : base(
            context
        ) { }

        // =====================================================================
        // IMessageBrokerApi
        // =====================================================================

        Task<ServiceCallResult> IMessageBrokerApi.IssueCommand(
            FabricMessage request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    Service.SendMessage(r, Log);
                    return Task.CompletedTask;
                }
            );

        Task<ServiceCallResult> IMessageBrokerApi.RaiseEvent(
            FabricMessage request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    Service.SendMessage(r, Log);
                    return Task.CompletedTask;
                }
            );

        Task<ServiceCallResult> IMessageBrokerApi.Subscribe(
            SubscribeRequest request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    request.Subscriptions.ForEach(s => Service.AddSubscription(s));
                    return Task.CompletedTask;
                }
            );

        Task<ServiceCallResult<WaitOnMessageResponse>> IMessageBrokerApi.WaitOnMessage(
            WaitOnMessageRequest request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    var b = Service.WaitForMessage(r.MessageId, (int)(r.WaitTimeoutSeconds * 1000f));
                    return Task.FromResult(new WaitOnMessageResponse {
                        Complete = b,
                        MessageId = r.MessageId
                    });
                }
            );
    }
}
