using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace TestServices.SvcWithDependency1; 

public interface ISvcWithDependency1 : IServiceApi {

    Task<ServiceCallResult<TestValueResponse>> GetTestValueWithDependency2Levels(
        TestValueRequest request
    );

    Task<ServiceCallResult> ChangeStaticValueWithDependency2Levels(TestValueRequest request);
}
