using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcGeneric.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests.ServiceCalls.SvcGeneric {

    public interface ISvcGenericApi : IServiceApi {

        Task<ServiceCallResult<TestValueResponse>> GetTestValueNoDependencies(
            TestValueRequest request
        );

        Task<ServiceCallResult<TestValueResponse>> Fails();

        Task<ServiceCallResult> ChangeStaticValue(TestValueRequest request);

    }
}