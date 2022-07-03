using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace TestServices.SvcGeneric; 

public interface ISvcGenericApi : IServiceApi {

    Task<ServiceCallResult<TestValueResponse>> GetTestValueNoDependencies(
        TestValueRequest request
    );

    Task<ServiceCallResult<TestValueResponse>> Fails();

    Task<ServiceCallResult> ChangeStaticValue(TestValueRequest request);

}
