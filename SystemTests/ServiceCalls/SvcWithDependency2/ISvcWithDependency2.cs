using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcWithDependency2.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests.ServiceCalls.SvcWithDependency2 {

    public interface ISvcWithDependency2 : IServiceApi {

        Task<ServiceCallResult<TestValueResponse>> GetTestValueWithDependency1Level(
            TestValueRequest request
        );

        Task<ServiceCallResult> ChangeStaticValueWithDependency1Level(TestValueRequest request);
    }
}