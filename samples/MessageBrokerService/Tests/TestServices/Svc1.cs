using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using System.Threading.Tasks;
using System;
using Tests.Services.MessageBroker.TestMessages;
using System.Collections.Generic;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker.Tests.TestServices;
public interface ISvc1Service : IManagedService, IServiceBase { 
    Guid TestValue { get; set; }
    string MessageName { get; set; }
}

	public class Svc1Service 
    : ManagedService<Svc1Operation>, ISvc1Service {

		protected override IReadOnlyDescriptor Descriptor => Constants.Service1;
    protected override IEnumerable<IReadOnlySubscription> EventSubscriptions => base.EventSubscriptions;
    
		public Svc1Service(
        IXKitHostEnvironment hostEnvironment
		) : base(hostEnvironment) { 
        
        AddEventSubscription<TestEvents>(x => x.Event1(null));
        AddEventSubscription<TestEvents>(x => x.Event2(null));
    }

    public Guid TestValue { get; set; }
    public string MessageName { get; set; }
	}

public partial class Svc1Operation : ServiceOperation<ISvc1Service>, TestEvents {

    public Svc1Operation(
        ServiceOperationContext context
    ) : base(context) { }

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
