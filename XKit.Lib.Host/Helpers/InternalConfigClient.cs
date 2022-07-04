using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Services;
using XKit.Lib.Common.Services.Config;
using XKit.Lib.Connector.Service;

namespace XKit.Lib.Host.Helpers {

    public interface IConfigClient : IServiceClient<IConfigApi>, IConfigApi { }

    public class InternalConfigClient : ServiceClientBase<IConfigApi>, IConfigClient {
        public InternalConfigClient(
            ILogSession log,
            IFabricConnector connector
        ) : base(
            StandardConstants.Managed.StandardServices.Config.Descriptor,
            log,
            connector,
            ServiceCallTypeParameters.SyncResult(),
            ServiceClientErrorHandling.DoNothing,
            null
        ) { }

        Task<ServiceCallResult<ConfigServiceQueryResponse>> IConfigApi.QueryConfig(ConfigServiceQueryRequest request)
            => ExecuteCall<ConfigServiceQueryRequest, ConfigServiceQueryResponse>(request);
    }
}
