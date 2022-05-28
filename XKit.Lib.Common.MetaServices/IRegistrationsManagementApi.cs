using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Common.MetaServices {

    public interface IRegistrationsManagementApi : IServiceApi {
        Task<ServiceCallResult> ResetTopologyMap(ServiceTopologyMap map);
        Task<ServiceCallResult> TriggerRefresh();
    }
}