using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using System.Threading.Tasks;
using System;
using Tests.Services.MessageBroker.TestMessages;

namespace Samples.MessageBroker.Tests.TestServices;
public interface ISvc2Service : IManagedService, IServiceBase { 
    Guid TestValue { get; set; }
    string MessageName { get; set; }
}

	public class Svc2Service 
    : ManagedService<Svc2Operation>, ISvc2Service {

		protected override IReadOnlyDescriptor Descriptor => Constants.Service2;

		public Svc2Service(
        IXKitHostEnvironment hostEnvironment
		) : base(hostEnvironment) { 
        AddEventSubscription<TestEvents>(x => x.Event2(null));
        AddCommandSubscription<TestCommands>(x => x.Command1(null));
    }

    public Guid TestValue { get; set; }
    public string MessageName { get; set; }
	}

public partial class Svc2Operation : ServiceOperation<ISvc2Service>, TestEvents, TestCommands {

    public Svc2Operation(
        ServiceOperationContext context
    ) : base(context) { }

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
}
