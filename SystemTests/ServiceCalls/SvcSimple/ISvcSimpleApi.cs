using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcSimple.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests.ServiceCalls.SvcSimple {

    public interface ISvcSimpleApi : IServiceApi {

        Task<ServiceCallResult<TestValueResponse>> GetTestValueNoDependencies(
            TestValueRequest request
        );

        Task<ServiceCallResult<TestValueResponse>> Fails();

        Task<ServiceCallResult> ChangeStaticValue(TestValueRequest request);

    }
}