using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace TestServices.SvcWithDependency2 {

    public interface ISvcWithDependency2 : IServiceApi {

        Task<ServiceCallResult<TestValueResponse>> GetTestValueWithDependency1Level(
            TestValueRequest request
        );

        Task<ServiceCallResult> ChangeStaticValueWithDependency1Level(TestValueRequest request);
    }
}
