using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using System.Threading.Tasks;
using System;
using Tests.Services.MessageBroker.TestMessages;

namespace Samples.MessageBroker.Tests.TestServices;
public interface ISvc4Service : IManagedService, IServiceBase { 
    Guid TestValue { get; set; }
    string MessageName { get; set; }
}

// NOTE:  This service will not automatically subscribe

	public class Svc4Service 
    : ManagedService<Svc4Operation>, ISvc4Service {

		protected override IReadOnlyDescriptor Descriptor => Constants.Service4;

		public Svc4Service(
        IXKitHostEnvironment hostEnvironment
		) : base(hostEnvironment) { }

    public Guid TestValue { get; set; }
    public string MessageName { get; set; }
	}

public partial class Svc4Operation : ServiceOperation<ISvc4Service>, TestCommands, TestEvents {

    public Svc4Operation(
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
                Service.TestValue = r.SomeValueGuid;
                Service.MessageName = "Command2";
                return Task.CompletedTask;
            }
        );
    }

    async Task<ServiceCallResult> TestEvents.Event1(TestPayload msg) {
        return await RunServiceCall(
            msg,
            operationAction: (r) => {
                Service.TestValue = r.SomeValueGuid;
                Service.MessageName = "Event1";
                return Task.CompletedTask;
            }
        );
    }

    async Task<ServiceCallResult> TestEvents.Event2(TestPayload msg) {
        return await RunServiceCall(
            msg,
            operationAction: (r) => {
                Service.TestValue = r.SomeValueGuid;
                Service.MessageName = "Event2";
                return Task.CompletedTask;
            }
        );
    }
}
