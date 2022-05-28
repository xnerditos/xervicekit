using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Common.Services.Config {
    public interface IConfigApi : IServiceApi {

        Task<ServiceCallResult<ConfigServiceQueryResponse>> QueryConfig(ConfigServiceQueryRequest request);
    }
}