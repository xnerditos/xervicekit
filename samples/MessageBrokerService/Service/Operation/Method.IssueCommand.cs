using System;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker; 
public partial class MessageBrokerOperation {
    Task<ServiceCallResult> IMessageBrokerApi.IssueCommand(FabricMessage request) 
        => RunServiceCall<FabricMessage>(
            requestBody: request,
            requestValidationAction: ValidateIssueCommand,
            preCallAction: (req) => PreOperationAction(true),
            operationAction : DoIssueCommand
        );

    private bool ValidateIssueCommand(FabricMessage request) 
        => !string.IsNullOrEmpty(request.MessageTypeName) && 
           request.MessageId != Guid.Empty;

    private Task DoIssueCommand(FabricMessage request) {
        var messageRecord = new Engine.MessageRecord {
            FabricMessage = new FabricMessage {
                JsonPayload = request.JsonPayload,
                MessageId = request.MessageId,
                MessageTypeName = request.MessageTypeName,
                OriginatorCorrelationId = request.OriginatorCorrelationId ?? this.Context.CorrelationId,
                OriginatorRequestorFabricId = request.OriginatorRequestorFabricId ?? this.Context.RequestorFabricId,
                OriginatorRequestorInstanceId = request.OriginatorRequestorInstanceId ?? this.Context.RequestorInstanceId
            },
            StartTimestamp = DateTime.UtcNow,
            Kind = MessageBroker.Engine.MessageKind.Command,
            LocalOperationId = this.Context.OperationId
        };

        var items = Engine.AddMessage(messageRecord);

        Service.SignalMessageAdded(messageRecord, items);
        return Task.CompletedTask;
    }
    
}
