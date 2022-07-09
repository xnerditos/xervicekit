using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
namespace TestServices.SvcListensForMessages; 

public partial class SvcListensForMessagesOperation 
    : ServiceOperation<ISvcListensForMessagesService>, 
    SvcSendsMessages.TestEvents, 
    SvcSendsMessages.TestCommands {

    public SvcListensForMessagesOperation(
        ServiceOperationContext context
    ) : base(context) { }

    Task<ServiceCallResult> SvcSendsMessages.TestEvents.Event1(
        SvcSendsMessages.Message request
    ) {
        return RunServiceCall(
            request,
            operationAction: (r) => {
                Service.TestValue = r.TheValue;
                Service.MessageName = nameof(SvcSendsMessages.TestEvents.Event1);
                return Task.CompletedTask;
            }
        );
    }

    Task<ServiceCallResult> SvcSendsMessages.TestCommands.Command1(
        SvcSendsMessages.Message request
    ) {
        return RunServiceCall(
            request,
            operationAction: (r) => {
                Service.TestValue = r.TheValue;
                Service.MessageName = nameof(SvcSendsMessages.TestCommands.Command1);
                return Task.CompletedTask;
            }
        );
    }

    Task<ServiceCallResult> SvcSendsMessages.TestCommands.Command2(
        SvcSendsMessages.Message request
    ) {
        return RunServiceCall(
            request,
            operationAction: (r) => {
                Thread.Sleep(5000);
                Service.TestValue = r.TheValue;
                Service.MessageName = nameof(SvcSendsMessages.TestCommands.Command2);
                return Task.CompletedTask;
            }
        );
    }
}
