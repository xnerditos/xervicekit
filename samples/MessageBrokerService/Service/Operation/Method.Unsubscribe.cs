using System.Linq;
using System.Threading.Tasks;
using Samples.MessageBroker.Engine;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker; 
public partial class MessageBrokerOperation {
    Task<ServiceCallResult> IMessageBrokerApi.Unsubscribe(UnsubscribeRequest request) 
        => RunServiceCall(
            requestBody: request,
            requestValidationAction: ValidateUnsubscribe,
            preCallAction: (req) => PreOperationAction(true),
            operationAction : DoUnsubscribe
        );

    private bool ValidateUnsubscribe(UnsubscribeRequest request) 
        =>  request.Subscriptions != null &&
            !request.Subscriptions.Any(
                s => string.IsNullOrEmpty(s.MessageTypeName) ||
                     s.Recipient == null ||
                     string.IsNullOrEmpty(s.Recipient.Collection) ||
                     string.IsNullOrEmpty(s.Recipient.Name)
            );

    private Task DoUnsubscribe(UnsubscribeRequest request) {
        foreach(var s in request.Subscriptions) {
            var queueName = s.GetQueueName();
            Engine.AbandonQueue(queueName);
        }
        return Task.CompletedTask;
    }
}
