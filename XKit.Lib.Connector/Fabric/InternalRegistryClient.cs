using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services;
using XKit.Lib.Common.Services.Registry;
using XKit.Lib.Connector.Service;

namespace XKit.Lib.Connector.Fabric {

    public interface IRegistryClient : IServiceClient<IRegistryApi>, IRegistryApi { }

    public class InternalRegistryClient : ServiceClientBase<IRegistryApi>, IRegistryClient {
        public InternalRegistryClient(
            ILogSession log,
            IDependencyConnector connector
        ) : base(
            StandardConstants.Managed.StandardServices.Registry.Descriptor,
            log,
            connector,
            ServiceCallTypeParameters.SyncResult(),
            ServiceClientErrorHandling.LogWarning,
            null
        ) { }

        Task<ServiceCallResult<ServiceTopologyMap>> IRegistryApi.Refresh(RefreshRegistrationRequest request) 
            => ExecuteCall<RefreshRegistrationRequest, ServiceTopologyMap>(request);

        Task<ServiceCallResult<ServiceTopologyMap>> IRegistryApi.Register(FabricRegistration request) 
            => ExecuteCall<FabricRegistration, ServiceTopologyMap>(request);

        Task<ServiceCallResult> IRegistryApi.Unregister(UnregisterRequest request)
            => ExecuteCall<UnregisterRequest>(request);
        
    }
}
