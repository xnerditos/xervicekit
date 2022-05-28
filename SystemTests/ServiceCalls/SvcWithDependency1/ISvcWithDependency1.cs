using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcWithDependency1.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests.ServiceCalls.SvcWithDependency1 {

    public interface ISvcWithDependency1 : IServiceApi {

        Task<ServiceCallResult<TestValueResponse>> GetTestValueWithDependency2Levels(
            TestValueRequest request
        );

        Task<ServiceCallResult> ChangeStaticValueWithDependency2Levels(TestValueRequest request);
    }
}