using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace TestServices.SvcSendsMessages; 

public interface TestCommands : IServiceCommands {

    Task<ServiceCallResult> Command1(Message request) => throw new System.NotImplementedException();
    Task<ServiceCallResult> Command2(Message request) => throw new System.NotImplementedException();
}
