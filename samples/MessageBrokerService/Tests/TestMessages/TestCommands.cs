using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace Tests.Services.MessageBroker.TestMessages; 
public interface TestCommands : IServiceCommands {

    Task<ServiceCallResult> Command1(TestPayload msg) => throw new NotImplementedException();
    Task<ServiceCallResult> Command2(TestPayload msg) => throw new NotImplementedException();
}
