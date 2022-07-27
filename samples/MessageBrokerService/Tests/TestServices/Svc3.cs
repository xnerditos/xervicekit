using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using System.Threading.Tasks;
using System;
using Tests.Services.MessageBroker.TestMessages;
using XKit.Lib.Common.Log;

namespace Samples.MessageBroker.Tests.TestServices;
public interface ISvc3Service : IManagedService, IServiceBase { 
    Guid TestValue { get; set; }
    string MessageName { get; set; }
    int Command2FailCount { get; set; }
}

	public class Svc3Service 
    : ManagedService<Svc3Operation>, ISvc3Service {

		protected override IReadOnlyDescriptor Descriptor => Constants.Service3;

		public Svc3Service(
        IXKitHostEnvironment hostEnvironment
		) : base(hostEnvironment) { 
        AddCommandSubscription<TestCommands>(x => x.Command1(null));
        AddCommandSubscription<TestCommands>(x => x.Command2(null));
    }

    public Guid TestValue { get; set; }
    public string MessageName { get; set; }
    public int Command2FailCount { get; set; } = 3;
	}

public partial class Svc3Operation : ServiceOperation<ISvc3Service>, TestCommands {

    public Svc3Operation(
        ServiceOperationContext context
    ) : base(context) { }

    async Task<ServiceCallResult> TestCommands.Command1(TestPayload msg) {
        return await RunServiceCall(
            msg,
            operationAction: (r) => {
                Service.TestValue = r.SomeValueGuid;
                Service.MessageName = "Command1";
                return Task.CompletedTask;
            }
        );
    }

    async Task<ServiceCallResult> TestCommands.Command2(TestPayload msg) {
        return await RunServiceCall(
            msg,
            operationAction: (r) => {
                if (Service.Command2FailCount > 0) {
                    Service.Command2FailCount--;
                    return Task.FromResult(new OperationResult {
                        OperationStatus = LogResultStatusEnum.RetriableError,
                        Message = "Test error: " + Service.Command2FailCount + 1
                    });
                }
                Service.TestValue = r.SomeValueGuid;
                Service.MessageName = "Command2";
                return Task.FromResult(
                    new OperationResult {
                        OperationStatus = LogResultStatusEnum.Success
                    }
                );
            }
        );
    }
}
