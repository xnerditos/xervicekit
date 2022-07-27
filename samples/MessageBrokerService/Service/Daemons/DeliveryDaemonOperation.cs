using System.Linq;
using System.Threading.Tasks;
using Mapster;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Connector.Service;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Samples.MessageBroker.Daemons; 
public class DeliveryDaemonOperation : ServiceDaemonOperation<DeliveryDaemonMessage> {

    private new IDeliveryDaemon Daemon => base.Daemon as IDeliveryDaemon;
    private new IMessageBrokerService Service => base.Service as IMessageBrokerService;

    public DeliveryDaemonOperation(
        ServiceDaemonOperationContext context
    ) : base(context) { }

    protected override Task<OperationResult> DoTimerOperation() {
        var engine = Service.GetMessageEngine();
        if (engine == null) { 
            Log.Erratum("Null message engine");
            return Task.FromResult(ResultNonRetriableError()); 
        }
        
        var items = engine.GetReadyQueueItemIds();
        foreach(var id in items) {
            Daemon.PostMessage(new() { QueueItemId = id }, triggerProcessing: false);
        }
        Daemon.ProcessMessages(background: true);
        return Task.FromResult(ResultSuccess());
    }

    protected override bool ValidateMessage(DeliveryDaemonMessage message) => true;

    protected override async Task<OperationResult> DoMessageOperation(DeliveryDaemonMessage message) {
        
        var engine = Service.GetMessageEngine();
        if (engine == null) { 
            Log.Erratum("Null message engine");
            return ResultNonRetriableError(); 
        }
        try {
            
            var config = await Service.GetConfig();
            engine.SetParameters(config?.DeliveryDaemon?.Adapt<MessageEngineParameters>());

            (var item, var subscription, var json) = engine.LockReadyQueueItem(message.QueueItemId);
            if (item == null) {
                Trace("Item not available: " + message.QueueItemId);
                return ResultNonRetriableError(); 
            }
            
            var messageNameParsed = item.MessageTypeName.Split('.');
            var client = ClientFactory.Factory.CreateGenericServiceClient(
                descriptor: subscription.Recipient,
                operationInterfaceName: messageNameParsed.Length < 2 ? null : messageNameParsed[0],
                log: Log,
                connector: Connector,
                defaultCallTypeParameters: ServiceCallTypeParameters.SyncResult(),
                errorHandling: subscription.ErrorHandling.GetValueOrDefault(ServiceClientErrorHandling.LogWarning),
                targetHostId: subscription.RecipientHostId
            );

            var results = await client.ExecuteCall(
                operationName: messageNameParsed[^1],
                requestJsonPayload: json,
                policy: subscription.Policy?.Clone(),
                callTypeParameters: ServiceCallTypeParameters.SyncResult()
            );

            bool isSuccess = results.Count > 0 && results.Any(r => !r.HasError);
            if (!isSuccess) {
                item = engine.FailQueueItem(item.QueueItemId, results);
                Daemon.AddWakeTime(item.ReadyInQueue);
            } else {
                engine.CompleteQueueItemAsDelivered(item.QueueItemId, results);
            }

        } catch {
            throw;
        } 
        return ResultSuccess();
    }
}
