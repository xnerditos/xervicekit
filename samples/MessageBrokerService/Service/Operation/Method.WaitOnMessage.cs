using System;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker; 
	public partial class MessageBrokerOperation {

    Task<ServiceCallResult<WaitOnMessageResponse>> IMessageBrokerApi.WaitOnMessage(WaitOnMessageRequest request) 
        => RunServiceCall<WaitOnMessageRequest, WaitOnMessageResponse>(
            requestBody: request,
            requestValidationAction: ValidateWaitOnMessage,
            preCallAction: (req) => PreOperationAction(false),
            operationAction : DoWaitOnMessage
        );

    private bool ValidateWaitOnMessage(WaitOnMessageRequest request) 
        => request.MessageId != Guid.Empty;

    private async Task<WaitOnMessageResponse> DoWaitOnMessage(WaitOnMessageRequest request) {
        var config = await Service.GetConfig();
        var waitTimeoutMilliseconds = 
            request.WaitTimeoutSeconds.HasValue ? request.WaitTimeoutSeconds.Value * 1000 : 
            config.WaitOnMessage.DefaultTimeoutMs.Value;
        var sleepWhileWaitingMilliseconds =
            config.WaitOnMessage.SleepWhileWaitingMs.Value;

        var timerBase = DateTime.UtcNow;
        var isFinished = Engine.IsMessageDelivered(request.MessageId);
        while (!isFinished && (DateTime.UtcNow - timerBase).TotalMilliseconds < waitTimeoutMilliseconds) {
            await Task.Delay(sleepWhileWaitingMilliseconds);
            isFinished = Engine.IsMessageDelivered(request.MessageId);
        }
        return new WaitOnMessageResponse { 
            MessageId = request.MessageId,
            Complete = isFinished,
            Results = Engine.GetMessage(request.MessageId).Results.ToArray()
        };
    }
}
