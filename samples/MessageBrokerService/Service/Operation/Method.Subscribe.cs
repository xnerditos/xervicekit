using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker; 
public partial class MessageBrokerOperation {
    Task<ServiceCallResult> IMessageBrokerApi.Subscribe(SubscribeRequest request) 
        => RunServiceCall(
            requestBody: request,
            requestValidationAction: ValidateSubscribe,
            preCallAction: (req) => PreOperationAction(true),
            operationAction : DoSubscribe
        );

    private bool ValidateSubscribe(SubscribeRequest request) 
        =>  request.Subscriptions != null &&
            !request.Subscriptions.Any(
                s => string.IsNullOrEmpty(s.MessageTypeName) ||
                     s.Recipient == null ||
                     string.IsNullOrEmpty(s.Recipient.Collection) ||
                     string.IsNullOrEmpty(s.Recipient.Name)
            );

    private Task DoSubscribe(SubscribeRequest request) {
        foreach(var s in request.Subscriptions) {
            Engine.CreateRenewSubscription(s);
        }
        return Task.CompletedTask;
    }
}
