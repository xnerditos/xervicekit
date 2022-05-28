using System.Threading.Tasks;
using SystemTests._NAMESPACE.Svc1.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests._NAMESPACE.Svc1 {

    public interface ISvc1Api : IServiceApi {

        Task<ServiceCallResult<TestValueResponse>> GetTestValue(
            TestValueRequest request
        );
    }
}