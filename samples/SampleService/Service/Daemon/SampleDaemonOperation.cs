
using System.Threading.Tasks;
using Samples.SampleService.V1.ServiceApiEntities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Samples.SampleService.V1; 

public class SampleDaemonOperation : ServiceDaemonOperation<SampleDaemonMessage> {

    public SampleDaemonOperation(
        ServiceDaemonOperationContext context
    ) : base(context) { }

    protected override Task DoTimerOperation() {
        // TODO:  Do something based on the timer
        return Task.CompletedTask;
    }

    protected override Task<OperationResult> DoMessageOperation(SampleDaemonMessage message) {
        // TODO:  Do something with the message
        return Task.FromResult(new OperationResult { OperationStatus = XKit.Lib.Common.Log.LogResultStatusEnum.Success });
    }
}
