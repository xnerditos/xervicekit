using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace Tests.Services.MessageBroker.TestMessages; 
public interface TestEvents : IServiceEvents {

    Task<ServiceCallResult> Event1(TestPayload msg) => throw new NotImplementedException();
    Task<ServiceCallResult> Event2(TestPayload msg) => throw new NotImplementedException();
}
