using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace TestServices.SvcSimple; 

public interface ISvcSimpleApi : IServiceApi {

    Task<ServiceCallResult<TestValueResponse>> GetTestValueNoDependencies(
        TestValueRequest request
    );

    Task<ServiceCallResult<TestValueResponse>> Fails();

    Task<ServiceCallResult> ChangeStaticValue(TestValueRequest request);

}
