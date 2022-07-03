using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace TestServices.SvcSendsMessages; 

public interface TestEvents : IServiceEvents {

    Task<ServiceCallResult> Event1(Message request) => throw new System.NotImplementedException();
    Task<ServiceCallResult> Event2(Message request) => throw new System.NotImplementedException();
}
