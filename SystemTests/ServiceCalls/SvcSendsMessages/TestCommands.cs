using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcSendsMessages.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests.ServiceCalls.SvcSendsMessages {

    public interface TestCommands : IServiceCommands {

        Task<ServiceCallResult> Command1(Message request) => throw new System.NotImplementedException();
        Task<ServiceCallResult> Command2(Message request) => throw new System.NotImplementedException();
    }
}