using System;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Services.Config;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Testing.TestConfigSvc {

    public partial class ConfigSvcOperation : ServiceOperation<IConfigSvcService>, IConfigApi {

        public ConfigSvcOperation(
            ServiceOperationContext context
        ) : base(
            context
        ) { }

        // =====================================================================
        // IConfigSvc
        // =====================================================================
        async Task<ServiceCallResult<ConfigServiceQueryResponse>> IConfigApi.QueryConfig(ConfigServiceQueryRequest request) 
            => await RunServiceCall(
                request,
                operationAction: QueryConfig
            );

        // =====================================================================
        // workers
        // =====================================================================

        private Task<ConfigServiceQueryResponse> QueryConfig(ConfigServiceQueryRequest request) {

            try {
                return Task.FromResult(new ConfigServiceQueryResponse {
                    HostConfig = Service.GetConfigForHost(),
                    ServiceConfigJson = request?.ServiceKeys?.ToDictionary(
                        k => k,
                        k => Service.GetConfigJsonForService(k)
                    )
                });
            } catch (Exception ex) {
                return Task.FromException<ConfigServiceQueryResponse>(ex);
            }
        }

        // =====================================================================
        // private
        // =====================================================================
    }
}
