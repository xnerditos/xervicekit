using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Services.Registry {
    public interface IRegistryApi : IServiceApi {

        Task<ServiceCallResult<ServiceTopologyMap>> Register(FabricRegistration request);
        Task<ServiceCallResult<ServiceTopologyMap>> Refresh(RefreshRegistrationRequest request);
        Task<ServiceCallResult> Unregister(UnregisterRequest request);
    }
}