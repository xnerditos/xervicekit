using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.Services;

namespace Samples.MessageBroker.Daemons; 
public partial class HousekeepingDaemonOperation
    : GenericTimerDaemonOperation {

    private new IMessageBrokerService Service => base.Service as IMessageBrokerService;

    public HousekeepingDaemonOperation(
        ServiceDaemonOperationContext context
    ) : base(context) { }

    protected override async Task<OperationResult> DoTimerOperation() {

        try { 
            var config = await Service.GetConfig();
            var deleteCompletedOlderThanMinutes = config.HousekeepingDaemon.DeleteCompletedItemsOlderThanXMinutes.Value;
            var deleteDeadOlderThanMinutes = config.HousekeepingDaemon.DeleteDeadItemsOlderThanXMinutes.Value;

            if (config.HousekeepingDaemon.Enable ?? true) {
                return ResultSuccess();
            }

            var completedOlderThan = DateTime.UtcNow.AddMinutes(deleteCompletedOlderThanMinutes * -1);
            var deadOlderThan = DateTime.UtcNow.AddMinutes(deleteDeadOlderThanMinutes * -1);
            var engine = Service.GetMessageEngine();

            engine.DeleteQueueItems(completedOlderThan, Engine.QueueItemState.Delivered);
            engine.DeleteQueueItems(deadOlderThan, Engine.QueueItemState.Dead);

        } catch(Exception ex) {
            Error(
                "Error doing housekeeping: " + ex.Message,
                new {
                    ex.StackTrace,
                    ex.TargetSite,
                    ex.Source
                });
            return ResultNonRetriableError(ex.Message);
        }

        return ResultSuccess();
    }
}
